# Maqar Public API

The API the Maqar public site (`maqar-properties-hub`) calls for listings, lookups, site contact details, the contact form and engagement tracking. It has no authentication: everything here is public, read-only data, plus two anonymous write endpoints that are rate-limited.

The response examples below come from the running API, with long text shortened (`"…"`) and some lists trimmed. Your IDs, image file names and dates will differ.

## Contents

- [Base URL](#base-url)
- [Conventions](#conventions)
- [Errors](#errors)
- [Rate limits](#rate-limits)
- [Endpoints](#endpoints)
  - [Listings](#get-apiproperties): `GET /api/properties`, `GET /api/properties/featured`, `GET /api/properties/{id}`, `GET /api/properties/{id}/similar`, `GET /api/properties/filters`
  - [Tracking](#post-apipropertiesidevents): `POST /api/properties/{id}/events`
  - [Lookups](#get-apiareas): `GET /api/areas`, `GET /api/features`
  - [Site](#get-apisite): `GET /api/site`
  - [Contact](#post-apicontact): `POST /api/contact`

---

## Base URL

| Environment | URL |
|---|---|
| Development (http) | `http://localhost:5026` |
| Development (https) | `https://localhost:7024` |
| Production | to be announced |

In Development, the interactive explorer is at **`/scalar`** and the OpenAPI document at **`/openapi/v1.json`**.

---

## Conventions

### JSON
- Requests and responses are `application/json; charset=utf-8`.
- Property names are camelCase.
- A POST body must be sent with `Content-Type: application/json`. Anything else gets **415**.

### Enum values are keys, not display text
Enums are sent and accepted as camelCase keys, the same strings the frontend already uses in `src/data/properties.ts`. Translate them with your i18n files.

| Field | Values |
|---|---|
| `type` | `apartment`, `villa`, `shop`, `land`, `building` |
| `listingType` | `sale`, `rent` |
| `rentalUnit` | `whole`, `room`, `bed` (rentals only, otherwise `null`) |
| `finishing` | `superLux`, `lux`, `semiFinished`, `core` |

Query parameters accept the same keys, case-insensitively (`?type=apartment`).

### Area and feature keys
- `area` and `features[]` on a listing are lookup **keys** (`"fifthSettlement"`, `"elevator"`).
- Their Arabic/English labels come from [`GET /api/areas`](#get-apiareas) and [`GET /api/features`](#get-apifeatures), or all at once from [`GET /api/properties/filters`](#get-apipropertiesfilters).
- Admins can add areas and features, so treat the lists as dynamic. A key you have no translation for should fall back to the API's `label`.

### Bilingual text: `Localized`
Every free-text field is an object with both languages. Pick the one you need on the client.

```json
{ "ar": "شقة بحديقة في التجمع", "en": "Garden Apartment" }
```

### Numbers and dates
- `price`, `size`, and the `coordinates` values are decimals and may carry trailing zeros (`4750000.00`, `30.007400`).
- `price` is in **EGP**: the total for a sale, the **monthly** amount for a rental.
- `size` is in **m²**. For a `room` or `bed` rental it is the size of that room.
- `createdAt` is a date string, `yyyy-MM-dd`.

### Nullable fields
- These can be `null`: `rentalUnit`, `bedrooms`, `bathrooms`, `finishing`, `floor`, `coordinates`.
- `bedrooms`, `bathrooms` and `finishing` are `null` for land and can be for shops. Render "—" or hide the row.

### Images
- `images[]` holds **absolute URLs**, cover image first, then the rest in display order.
- Use them as-is in `<img src>`. Every published listing has at least one image.

### Paged responses
List endpoints that page return this envelope:

```json
{
  "pageIndex": 1,
  "pageSize": 6,
  "count": 14,
  "totalPages": 3,
  "data": [ /* PropertyDto, … */ ]
}
```

| Field | Meaning |
|---|---|
| `pageIndex` | The page you got (1-based). |
| `pageSize` | Items per page. |
| `count` | Total matches across all pages. |
| `totalPages` | `ceil(count / pageSize)`; `0` when nothing matches. |
| `data` | This page's items. An empty array past the last page is **not** an error. |

### The `PropertyDto` object
Every listing endpoint returns this shape, both in lists and on the details endpoint:

```json
{
  "id": 7,
  "type": "apartment",
  "listingType": "sale",
  "rentalUnit": null,
  "area": "fifthSettlement",
  "featured": true,
  "price": 4750000.00,
  "isNegotiable": true,
  "bedrooms": 3,
  "bathrooms": 2,
  "size": 180.00,
  "finishing": "superLux",
  "floor": null,
  "createdAt": "2026-10-16",
  "images": [
    "https://localhost:7069/Files/Properties/61b2e7e0-06af-474f-a7e9-9a475910b42f.jpg",
    "https://localhost:7069/Files/Properties/7726df8a-4359-4735-93cc-63a3a47ba7a5.jpg"
  ],
  "features": ["elevator", "ac"],
  "title": { "ar": "شقة بحديقة في التجمع", "en": "Garden Apartment" },
  "location": { "ar": "شارع التسعين", "en": "90th Street" },
  "description": { "ar": "وصف الشقة", "en": "Bright apartment with a private garden." },
  "coordinates": { "lat": 30.007400, "lng": 31.491300 },
  "contact": { "phone": "01011111111", "whatsapp": "201022222222" }
}
```

| Field | Type | Notes |
|---|---|---|
| `id` | number | |
| `type` | string | See [enums](#enum-values-are-keys-not-display-text). |
| `listingType` | string | `sale` \| `rent` |
| `rentalUnit` | string \| null | `whole` \| `room` \| `bed`; only for rentals. |
| `area` | string | Area key. |
| `featured` | boolean | Shown in the home page's featured section. |
| `price` | number | EGP; monthly when `listingType` is `rent`. |
| `isNegotiable` | boolean | |
| `bedrooms`, `bathrooms` | number \| null | |
| `size` | number | m². |
| `finishing` | string \| null | |
| `floor` | number \| null | `0` = ground floor, negative = basement. |
| `createdAt` | string | `yyyy-MM-dd` |
| `images` | string[] | Absolute URLs, cover first. |
| `features` | string[] | Feature keys. |
| `title`, `location`, `description` | Localized | `location` is the street / compound line. The district is `area`. |
| `coordinates` | `{ lat, lng }` \| null | For a map pin. |
| `contact` | `{ phone, whatsapp }` | Who the Call and WhatsApp buttons reach. See below. |

**`contact`** is the listing's broker:
- `phone` is the broker's phone number. Use it in `tel:` links.
- `whatsapp` is the broker's WhatsApp number in the digits-only international form `wa.me` expects: `https://wa.me/{whatsapp}`.
- If the broker has no number or is no longer active, both fall back to the company numbers from [`GET /api/site`](#get-apisite). Either way the field is always filled.

---

## Errors

Every error has the same JSON body, so one error handler covers the whole API.

**`ApiResponse`**, used for most errors:
```json
{ "statusCode": 404, "message": "Property not found." }
```

**`ApiValidationErrorResponse`**, used for `400`. `errors` lists every problem:
```json
{
  "errors": ["The Email field is not a valid e-mail address."],
  "statusCode": 400,
  "message": "Bad Request"
}
```

**`ApiExceptionResponse`**, used for `500`. `details` is a stack trace in Development and `null` in Production:
```json
{ "details": null, "statusCode": 500, "message": "Internal Server Error" }
```

| Status | When | Body |
|---|---|---|
| `400` | Invalid query value (e.g. `?type=castle`), invalid or missing body field, or a body that isn't valid JSON | `ApiValidationErrorResponse` |
| `404` | A listing that doesn't exist **or isn't published** (draft, sold, rented, archived, deleted), an unknown route, or a non-numeric id (`/api/properties/abc`) | `ApiResponse` |
| `405` | Wrong HTTP method for an existing route (e.g. `DELETE /api/site`) | `ApiResponse` |
| `415` | POST without `Content-Type: application/json` | `ApiResponse` |
| `429` | Rate limit exceeded (see below) | `ApiResponse`, plus a `Retry-After` header |
| `500` | Unexpected server error | `ApiExceptionResponse` |

The standard messages for each status:

| Status | `message` |
|---|---|
| 400 | `"Bad Request"` |
| 404 | `"Resource Not Found"` for an unknown route, or a specific message such as `"Property not found."` |
| 405 | `"Method Not Allowed"` |
| 415 | `"Unsupported media type. Send the body as application/json."` |
| 429 | `"Too many requests. Please wait a few minutes and try again."` |
| 500 | `"Internal Server Error"` |

Treat `errors[]` and `message` as English text for logs and fallbacks. Show your own translated messages in the UI.

---

## Rate limits

Limits are per client IP, over a fixed window. **Every request counts**, including ones rejected as invalid.

| Endpoint | Limit |
|---|---|
| `POST /api/contact` | 5 requests per 10 minutes |
| `POST /api/properties/{id}/events` | 60 requests per minute |

When a limit is exceeded you get **429** with `Retry-After: <seconds>`:

```http
HTTP/1.1 429 Too Many Requests
Retry-After: 600
Content-Type: application/json

{ "statusCode": 429, "message": "Too many requests. Please wait a few minutes and try again." }
```

The GET endpoints are not rate-limited.

---

## Endpoints

### `GET /api/properties`

Search published listings, with filtering, sorting and paging done on the server. The query parameter names match the frontend's existing `/properties` URL search params, so they can be passed straight through.

**Query parameters**

| Name | Type | Allowed values | Default | Effect |
|---|---|---|---|---|
| `area` | string | an area key | — | Listings in that area. An unknown key returns no results, not an error. |
| `type` | string | `apartment` `villa` `shop` `land` `building` | — | |
| `listingType` | string | `sale` `rent` | — | |
| `rentalUnit` | string | `whole` `room` `bed` | — | Only meaningful with `listingType=rent`. |
| `minPrice` / `maxPrice` | number | ≥ 0 | — | Inclusive. Compare against monthly rent for rentals. |
| `minSize` / `maxSize` | number | ≥ 0 | — | m², inclusive. |
| `bedrooms` | integer | ≥ 0 | — | **At least** this many. Listings without bedrooms (land) are excluded. |
| `sort` | string | `newest` `asc` `desc` | `newest` | `asc` / `desc` sort by price. An unknown value falls back to `newest`. |
| `page` | integer | ≥ 1 | `1` | Values below 1 become 1. |
| `pageSize` | integer | 1–24 | `6` | Values above 24 are capped at 24. |

**Example**

```http
GET /api/properties?listingType=rent&rentalUnit=room&maxPrice=8000&sort=asc&page=1
```

**200 OK**, a [paged](#paged-responses) list of [`PropertyDto`](#the-propertydto-object):

```json
{
  "pageIndex": 1,
  "pageSize": 6,
  "count": 1,
  "totalPages": 1,
  "data": [
    {
      "id": 9,
      "type": "apartment",
      "listingType": "rent",
      "rentalUnit": "room",
      "area": "maadi",
      "featured": true,
      "price": 6000.00,
      "isNegotiable": true,
      "bedrooms": 1,
      "bathrooms": 1,
      "size": 25.00,
      "finishing": "lux",
      "floor": null,
      "createdAt": "2026-10-16",
      "images": ["https://localhost:7069/Files/Properties/4f866d6c-e13c-4686-a1f4-4774b824fb95.jpg"],
      "features": ["security", "ac"],
      "title": { "ar": "غرفة في المعادي", "en": "Maadi Room" },
      "location": { "ar": "شارع ٩", "en": "Street 9" },
      "description": { "ar": "…", "en": "…" },
      "coordinates": null,
      "contact": { "phone": "01033333333", "whatsapp": "201033333333" }
    }
  ]
}
```

**Nothing matches, or the page is past the end.** This is still `200`:
```json
{ "pageIndex": 9, "pageSize": 2, "count": 4, "totalPages": 2, "data": [] }
```

**400 Bad Request**, when an enum or number parameter can't be parsed:
```json
{ "errors": ["The value 'castle' is not valid for Type."], "statusCode": 400, "message": "Bad Request" }
```
```json
{ "errors": ["The value 'abc' is not valid for MinPrice."], "statusCode": 400, "message": "Bad Request" }
```

---

### `GET /api/properties/featured`

Published listings marked as featured, newest first. Use it for the home page's "Featured properties" section.

| Query | Type | Allowed | Default |
|---|---|---|---|
| `take` | integer | 1–12 (clamped) | `6` |

```http
GET /api/properties/featured?take=6
```

**200 OK**: an array of [`PropertyDto`](#the-propertydto-object), not paged. It is `[]` when nothing is featured.

```json
[
  { "id": 9, "type": "apartment", "listingType": "rent", "featured": true, "…": "…" },
  { "id": 7, "type": "apartment", "listingType": "sale", "featured": true, "…": "…" }
]
```

---

### `GET /api/properties/{id}`

One published listing, for the details page.

| Path | Type |
|---|---|
| `id` | integer |

```http
GET /api/properties/7
```

**200 OK**: a [`PropertyDto`](#the-propertydto-object), the same full example as above.

**404 Not Found**: the listing doesn't exist, or isn't public (a draft, or sold, rented, archived or deleted). The API doesn't say which.
```json
{ "statusCode": 404, "message": "Property not found." }
```

**404 Not Found** for a non-numeric id (`/api/properties/abc`); no route matches:
```json
{ "statusCode": 404, "message": "Resource Not Found" }
```

> To show a "Sold" or "Rented" ribbon instead of a 404 for those statuses, ask the backend to expose them. Today they are hidden.

---

### `GET /api/properties/{id}/similar`

The "You may also like" section on a details page: published listings with the **same listing type**, and the **same area or the same property type**. Excludes the listing itself; newest first.

| Path / Query | Type | Allowed | Default |
|---|---|---|---|
| `id` | integer | | |
| `take` | integer | 1–8 (clamped) | `4` |

```http
GET /api/properties/7/similar?take=4
```

**200 OK**: an array of [`PropertyDto`](#the-propertydto-object). It is `[]` when nothing is similar, so hide the section in that case.

**404 Not Found**: the source listing isn't public.
```json
{ "statusCode": 404, "message": "Property not found." }
```

---

### `GET /api/properties/filters`

Everything the filter bar needs in one call:
- the active areas and features with labels
- every enum key
- the **real** price and size bounds of published listings, for slider limits and placeholders.

```http
GET /api/properties/filters
```

**200 OK**

```json
{
  "areas": [
    { "key": "fifthSettlement", "label": { "ar": "التجمع الخامس", "en": "Fifth Settlement" }, "icon": null },
    { "key": "maadi", "label": { "ar": "المعادي", "en": "Maadi" }, "icon": null },
    { "key": "sheikhZayed", "label": { "ar": "الشيخ زايد", "en": "Sheikh Zayed" }, "icon": null },
    { "key": "nasrCity", "label": { "ar": "مدينة نصر", "en": "Nasr City" }, "icon": null },
    { "key": "october", "label": { "ar": "6 أكتوبر", "en": "6th of October" }, "icon": null }
  ],
  "features": [
    { "key": "elevator", "label": { "ar": "مصعد", "en": "Elevator" }, "icon": null },
    { "key": "security", "label": { "ar": "حارس أمن", "en": "Security" }, "icon": null }
  ],
  "propertyTypes": ["apartment", "villa", "shop", "land", "building"],
  "listingTypes": ["sale", "rent"],
  "rentalUnits": ["whole", "room", "bed"],
  "finishings": ["superLux", "lux", "semiFinished", "core"],
  "price": {
    "sale": { "min": 3000000.00, "max": 12500000.00 },
    "rent": { "min": 6000.00, "max": 6000.00 }
  },
  "size": { "min": 25.00, "max": 600.00 }
}
```

(`features` is shortened here. The real response lists all active features.)

**Fields that can be `null`:**
- `price.sale` is `null` when nothing is published for sale.
- `price.rent` is `null` when nothing is published for rent.
- `size` is `null` when nothing is published at all.

Fall back to your own defaults when they're missing. `icon` is an admin-dashboard icon class; the public site can ignore it.

---

### `POST /api/properties/{id}/events`

Records an engagement event for the admin dashboard's statistics:
- a **view** of a listing's details page
- a tap on the **WhatsApp** button
- a tap on the **Call** button.

| Path | Type |
|---|---|
| `id` | integer |

**Body**

| Field | Type | Allowed | Required |
|---|---|---|---|
| `type` | string | `view`, `whatsapp`, `call` | yes |

```http
POST /api/properties/7/events
Content-Type: application/json

{ "type": "view" }
```

**204 No Content**: accepted, with no body. Repeats are **accepted but not counted**: the same visitor (IP + browser) doing the same thing to the same listing within **30 minutes** counts once. Refreshes and double taps are therefore harmless, and you don't need to debounce on the client.

**400 Bad Request**

Unknown `type`:
```json
{ "errors": ["type must be one of: view, whatsapp, call."], "statusCode": 400, "message": "Bad Request" }
```

Missing `type`:
```json
{ "errors": ["type is required: view, whatsapp or call."], "statusCode": 400, "message": "Bad Request" }
```

Body isn't valid JSON:
```json
{ "errors": ["The request body is not valid JSON."], "statusCode": 400, "message": "Bad Request" }
```

**404 Not Found**: the listing isn't public.
```json
{ "statusCode": 404, "message": "Property not found." }
```

**415 Unsupported Media Type**: `Content-Type` isn't `application/json`.
```json
{ "statusCode": 415, "message": "Unsupported media type. Send the body as application/json." }
```

**429 Too Many Requests**: more than 60 events per minute from one IP. See [Rate limits](#rate-limits).

---

### `GET /api/areas`

Active areas in display order.

**200 OK**
```json
[
  { "key": "fifthSettlement", "label": { "ar": "التجمع الخامس", "en": "Fifth Settlement" }, "icon": null },
  { "key": "maadi", "label": { "ar": "المعادي", "en": "Maadi" }, "icon": null }
]
```

Deactivated areas are not listed, but existing listings may still reference them. If you meet an area key that isn't in the list, show the key or "—".

---

### `GET /api/features`

Active amenities in display order. The response shape is the same as areas.

**200 OK**
```json
[
  { "key": "elevator", "label": { "ar": "مصعد", "en": "Elevator" }, "icon": null },
  { "key": "pool", "label": { "ar": "حمام سباحة", "en": "Swimming pool" }, "icon": null }
]
```

---

### `GET /api/site`

The company's contact details, managed in the admin dashboard under **Site Settings**. Use them for the footer and the contact page. They replace the hardcoded `SITE` constant.

**200 OK**
```json
{
  "phone": "+201234567890",
  "phoneDisplay": "+20 123 456 7890",
  "whatsapp": "201234567890",
  "email": "info@maqar.eg",
  "address": { "ar": "", "en": "90th Street, New Cairo" },
  "facebookUrl": "https://facebook.com/maqar",
  "instagramUrl": null
}
```

| Field | Notes |
|---|---|
| `phone` | For `tel:` links. |
| `phoneDisplay` | How to print the number. |
| `whatsapp` | Digits only: `https://wa.me/{whatsapp}`. |
| `address` | `null` when neither language is set. One side can be an empty string when only the other language is filled in. |
| `facebookUrl`, `instagramUrl` | `null` when not set; hide the icon. |

---

### `POST /api/contact`

Sends the contact form. With `propertyId` set, it sends an **inquiry about a listing**, which also appears in that listing's broker's inbox.

**Body**

| Field | Type | Rules | Required |
|---|---|---|---|
| `name` | string | 2–100 characters | yes |
| `email` | string | a valid email, ≤ 256 | yes |
| `phone` | string | 6–30 characters: digits, spaces, `-`, `(`, `)`, optional leading `+` | yes |
| `message` | string | 5–2000 characters | yes |
| `propertyId` | number | a **published** listing id | no |

```http
POST /api/contact
Content-Type: application/json

{
  "name": "Sara Ali",
  "email": "sara@example.com",
  "phone": "+20 100 123 4567",
  "message": "Is the garden apartment still available?",
  "propertyId": 7
}
```

**201 Created**: the new message id.
```json
{ "id": 5 }
```

**400 Bad Request**: one entry per failed rule. For example, `{"name":"","email":"not-an-email","phone":"abc","message":"hi"}` gives:
```json
{
  "errors": [
    "The Name field is required.",
    "The field Name must be a string with a minimum length of 2 and a maximum length of 100.",
    "The Email field is not a valid e-mail address.",
    "The field Phone must be a string with a minimum length of 6 and a maximum length of 30.",
    "The Phone field must contain digits, spaces, dashes or a leading +.",
    "The field Message must be a string with a minimum length of 5 and a maximum length of 2000."
  ],
  "statusCode": 400,
  "message": "Bad Request"
}
```

**404 Not Found**: `propertyId` was sent but the listing isn't public. The message is **not** saved.
```json
{ "statusCode": 404, "message": "The listing this inquiry is about is no longer available." }
```

**415 Unsupported Media Type**: the body wasn't sent as JSON.

**429 Too Many Requests**: more than 5 submissions in 10 minutes from one IP, invalid ones included. Show the user a "please try again later" message.
```json
{ "statusCode": 429, "message": "Too many requests. Please wait a few minutes and try again." }
```

---

*Last updated: 2026-10-17.*
