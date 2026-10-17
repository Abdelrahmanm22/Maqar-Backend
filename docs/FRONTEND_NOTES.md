# Frontend notes: switching `maqar-properties-hub` to the real API

The backend is ready. This note lists every change the site needs to stop using mock data, file by file. Each endpoint, with all its responses, is documented in **[API.md](API.md)**.

**In short:**
- Swap the two mock fetchers for real calls.
- Move filtering and paging to the server.
- Read contact numbers from the API.
- Send three tracking events.
- Most components keep working, because the response mirrors your existing `Property` type.

---

## 1. Configuration

Add the API base URL as an env var.

```bash
# .env.local
VITE_API_BASE_URL=http://localhost:5026
```

**CORS:** the API currently allows browser calls only from `http://localhost:8080` and `http://localhost:5173`. Send us:
- your production and preview domains, so we add them
- your dev origin, if it differs from those two.

**SSR:** TanStack Start also fetches on the server during SSR. CORS doesn't apply there, but that server must be able to reach `VITE_API_BASE_URL`. In production, use a URL that is reachable both publicly and from the SSR runtime.

## 2. A small fetch helper

Every error from the API has the same shape (`{ statusCode, message, errors?, details? }`), so one helper covers them all:

```ts
// src/lib/api.ts
const BASE = import.meta.env.VITE_API_BASE_URL as string;

export class ApiError extends Error {
  constructor(
    public status: number,
    message: string,
    public errors: string[] = [],
    public retryAfter?: number,
  ) {
    super(message);
  }
}

export async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", ...init?.headers },
  });

  if (res.status === 204) return undefined as T;

  const body = await res.json().catch(() => null);
  if (!res.ok) {
    const retry = Number(res.headers.get("Retry-After")) || undefined;
    throw new ApiError(res.status, body?.message ?? res.statusText, body?.errors ?? [], retry);
  }
  return body as T;
}

/** Turns the frontend's search object into a query string, skipping empty values. */
export function toQuery(params: Record<string, unknown>): string {
  const q = new URLSearchParams();
  for (const [k, v] of Object.entries(params)) {
    if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
  }
  const s = q.toString();
  return s ? `?${s}` : "";
}
```

## 3. Type changes in `src/data/properties.ts`

The API returns your `Property` shape, with these differences:

| Field | Before | Now | Why |
|---|---|---|---|
| `id` | `string` | `number` | Database id. Use `String(p.id)` in route params, or parse `Number(params.id)`. |
| `bedrooms`, `bathrooms` | `number` | `number \| null` | `null` for land, and possibly for shops. |
| `finishing` | `FinishingKey` | `FinishingKey \| null` | Same reason. |
| `area` | `AreaKey` (fixed union) | `string` | Admins can add areas. Keep `AreaKey` for known ones, but accept any string. |
| `features` | `FeatureKey[]` | `string[]` | Admins can add features. |
| *new* `isNegotiable` | | `boolean` | Show a "negotiable" hint next to the price if you like. |
| *new* `floor` | | `number \| null` | `0` = ground floor. |
| *new* `coordinates` | | `{ lat: number; lng: number } \| null` | Replaces the map placeholder. |
| *new* `contact` | | `{ phone: string; whatsapp: string }` | The listing's broker. See §7. |

```ts
export interface Property {
  id: number;
  type: PropertyType;
  listingType: ListingType;
  rentalUnit: RentalUnitKey | null;
  area: string;
  featured: boolean;
  price: number;
  isNegotiable: boolean;
  bedrooms: number | null;
  bathrooms: number | null;
  size: number;
  finishing: FinishingKey | null;
  floor: number | null;
  createdAt: string; // yyyy-MM-dd
  images: string[]; // absolute URLs, cover first
  features: string[];
  title: Localized;
  location: Localized;
  description: Localized;
  coordinates: { lat: number; lng: number } | null;
  contact: { phone: string; whatsapp: string };
}

export interface Paged<T> {
  pageIndex: number;
  pageSize: number;
  count: number;
  totalPages: number;
  data: T[];
}
```

**Places that assume numbers today:**
- **`PropertyCard.tsx`:** `property.bedrooms > 0` is fine with `null` in JS but fails strict TypeScript. Use `(property.bedrooms ?? 0) > 0`.
- **`properties.$id.tsx`:**
  - `fmtNum(property.bedrooms)` and the bathrooms spec need a "—" fallback.
  - `t(\`finishing.${property.finishing}\`)` must be skipped when `finishing` is `null`.
  - `property.bathrooms === 0` should become `!property.bathrooms`.
- **`isRoomOrBed()`:** works unchanged.

The `AREAS` constant and the other option lists can go; see §5.

## 4. Replace the mock fetchers

Delete `PROPERTIES` and the fake delays, then point the two functions at the API.

```ts
import { api, toQuery } from "@/lib/api";

export type ListingSearch = {
  area?: string; type?: string; listingType?: string; rentalUnit?: string;
  minPrice?: number; maxPrice?: number; minSize?: number; maxSize?: number;
  bedrooms?: number; sort?: "newest" | "asc" | "desc"; page?: number;
};

export const fetchProperties = (search: ListingSearch, pageSize = 6) =>
  api<Paged<Property>>(`/api/properties${toQuery({ ...search, pageSize })}`);

export const fetchFeatured = (take = 6) =>
  api<Property[]>(`/api/properties/featured?take=${take}`);

/** Resolves to undefined on 404, so the existing "not found" UI keeps working. */
export async function fetchProperty(id: string | number): Promise<Property | undefined> {
  try {
    return await api<Property>(`/api/properties/${id}`);
  } catch (e) {
    if (e instanceof ApiError && e.status === 404) return undefined;
    throw e;
  }
}

export const fetchSimilar = (id: string | number, take = 4) =>
  api<Property[]>(`/api/properties/${id}/similar?take=${take}`);
```

A 404 covers drafts, sold, rented and archived listings too, so an old shared link lands on your not-found state.

### Home page (`routes/index.tsx`)
```ts
const { data, isLoading } = useQuery({ queryKey: ["properties", "featured"], queryFn: () => fetchFeatured(6) });
// remove: .filter((p) => p.featured).slice(0, 6), which the server now does
```

### Listings page (`routes/properties.index.tsx`)
- **Remove the client-side code:** the `filtered` / `sorted` `useMemo`, `PAGE_SIZE` slicing and `totalPages` math. The server does all of it with the **same parameter names** you already keep in the URL.
- **Move `sort` into the URL.** It's currently a `useState`, so it resets on reload and isn't shareable. Values: `newest`, `asc`, `desc`, as you already use.

```ts
const search = Route.useSearch(); // area, type, listingType, …, sort, page
const { data, isLoading } = useQuery({
  queryKey: ["properties", search],
  queryFn: () => fetchProperties(search),
  placeholderData: keepPreviousData, // no flicker while paging
});
const current = data?.data ?? [];
const totalPages = Math.max(1, data?.totalPages ?? 1);
const total = data?.count ?? 0; // "N results"
```

- **`bedrooms`** means "at least N" on the server too, and land is excluded when it's set. That matches your current filter.
- **A page past the end** returns `200` with `data: []`, not an error. Your empty state handles it.

### Details page (`routes/properties.$id.tsx`)
- `fetchProperty(id)` keeps its signature. `id` from the route is a string, which is fine because it goes into the URL.
- **"You may also like":** `fetchSimilar(id)` returns `[]` when nothing fits, so hide the section then. Reuse `PropertyCard`.
- **Map:** `property.coordinates` (nullable) replaces the placeholder. Hide the map when it's `null`.

## 5. Filter options and slider bounds

`GET /api/properties/filters` returns, in one call:
- the active `areas` and `features` with `{ key, label: { ar, en } }`
- every enum key
- `price.sale`, `price.rent` and `size`, each as `{ min, max }`: the real bounds of published listings. Each can be `null` when no listing exists for it.

```ts
const { data: filters } = useQuery({
  queryKey: ["filters"],
  queryFn: () => api<FilterMetadata>("/api/properties/filters"),
  staleTime: 10 * 60_000,
});
```

**How to use it:**
- **Area dropdown:** build it from `filters.areas`. Use your i18n key when you have one (`t(\`area.${key}\`)`), and fall back to `label[lang]` for areas an admin added later.
- **Sliders and placeholders:** take their bounds from `filters.price[listingType]` / `filters.size` instead of hardcoded numbers.
- **The `SearchBar` area select on the home page** can use the same query.

## 6. Site contact details: replace `src/lib/site.ts`

The phone, WhatsApp, email, address and social links are now managed in the dashboard.

```ts
// src/lib/site.ts
export type SiteSettings = {
  phone: string; phoneDisplay: string; whatsapp: string; email: string;
  address: { ar: string; en: string } | null;
  facebookUrl: string | null; instagramUrl: string | null;
};

export const useSite = () =>
  useQuery({ queryKey: ["site"], queryFn: () => api<SiteSettings>("/api/site"), staleTime: 30 * 60_000 });

export const whatsappLink = (number: string) => `https://wa.me/${number}`;
```

| Where | Change |
|---|---|
| `Footer.tsx` | `SITE.*` → `site.data?.*`; `whatsappUrl` → `whatsappLink(site.whatsapp)`. Optionally show the address and social icons when they're non-null. |
| `routes/contact.tsx` | Same as the footer. |
| `WhatsAppButton.tsx` (floating) | The **company** number: `site.whatsapp`. |

Keep the current values as a fallback while the query loads, so the footer never renders empty.

## 7. Call and WhatsApp buttons on a listing → `property.contact`

On the details page, **don't use the site numbers**. Each listing carries `contact`, which reaches the broker who listed it. The backend already falls back to the company numbers when the broker has none or is inactive, so it is always filled.

```tsx
<a href={whatsappLink(property.contact.whatsapp)} onClick={() => track(property.id, "whatsapp")}>…</a>
<a href={`tel:${property.contact.phone}`} onClick={() => track(property.id, "call")}>…</a>
```

`contact.whatsapp` is already digits-only in international form, ready for `wa.me`.

## 8. Contact form (`routes/contact.tsx`)

It currently only sets `sent = true`. POST it instead:

```ts
const send = useMutation({
  mutationFn: (body: { name: string; email: string; phone: string; message: string; propertyId?: number }) =>
    api<{ id: number }>("/api/contact", { method: "POST", body: JSON.stringify(body) }),
  onSuccess: () => setSent(true),
});
```

**Responses to handle:**

| Status | Show |
|---|---|
| `201` | The success state, as today. |
| `400` | Field errors. `error.errors` is a list of English messages; map them to your localized field errors, or show a generic "check the highlighted fields". Validate on the client with zod using the same rules: name 2–100, valid email, phone 6–30 of digits/space/`-`/`()`/leading `+`, message 5–2000. |
| `404` | Only when `propertyId` was sent and that listing is gone: "This listing is no longer available." |
| `429` | "Too many messages, please try again in a few minutes." `error.retryAfter` is in seconds. **Every** submission counts toward the limit (5 per 10 minutes per IP), invalid ones included. |

**Optional:** add an inquiry form on the details page that sends the same body plus `propertyId: property.id`. Those messages go straight to the listing's broker in the dashboard.

## 9. Engagement tracking (powers the dashboard statistics)

Send three events with `POST /api/properties/{id}/events` and body `{ "type": … }`:

| `type` | When |
|---|---|
| `view` | Once when the details page **mounts in the browser**. |
| `whatsapp` | On click of the listing's WhatsApp button. |
| `call` | On click of the listing's Call button. |

```ts
// src/lib/track.ts
export function track(id: number | string, type: "view" | "whatsapp" | "call") {
  fetch(`${import.meta.env.VITE_API_BASE_URL}/api/properties/${id}/events`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ type }),
    keepalive: true, // survives the navigation to wa.me / tel:
  }).catch(() => {}); // tracking must never break the page
}

// properties.$id.tsx
useEffect(() => { track(id, "view"); }, [id]); // useEffect only runs in the browser, never during SSR
```

**Important:**
- **Don't send `view` from the route loader or during SSR.** The server would be counted as the visitor, and every SSR render would add a view.
- **No debouncing needed.** The API counts the same visitor once per listing per type per 30 minutes, and still answers `204`.
- **Fire and forget.** Ignore the response.
- **Don't track the floating company WhatsApp button.** It isn't tied to a listing.

## 10. React Query keys (suggested)

| Data | Key | `staleTime` |
|---|---|---|
| Listing page | `["properties", search]` | default |
| Featured | `["properties", "featured"]` | 5 min |
| One listing | `["property", id]` | default |
| Similar | `["property", id, "similar"]` | 5 min |
| Filters | `["filters"]` | 10 min |
| Site | `["site"]` | 30 min |

## 11. Unchanged

- **i18n and RTL:** text comes as `{ ar, en }` exactly like the mock data, and enum and area keys still match your translation keys.
- **`Localized`, `fmtPrice`, the "/ month" suffix (`listingType === "rent"`), `isRoomOrBed`.**
- **Image URLs** are absolute and ready for `<img src>`, cover first. Every published listing has at least one image.

## 12. Questions for you

- **Your production domain(s), for CORS.**
- **Sold and rented listings:** should a sold or rented listing still be viewable with a ribbon? Today they return 404.
- **Missing fields:** anything the UI needs that isn't in the payload yet.
