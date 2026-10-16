# CLAUDE.md — Maqar Properties Hub (Backend API + Admin Dashboard)

## Project Overview

The **backend** for **Maqar (مَقَر)**, an Egyptian real-estate listings site. It has two web apps:

- **`Maqar.Admin`**: a **.NET 9 MVC** admin dashboard (server-rendered Razor, Skote template). Staff manage listings, users and roles here.
- **`Maqar.Api`**: a **.NET 9 Web API** that the public site will call. No endpoints exist yet; the project is wired to the layers and waiting for them.

The public site is a separate repo: **`D:\My Career\maqar-properties-hub`**. It is TanStack Start + React 19, Arabic/English with RTL, and currently runs on mock data. Its data shapes are the **contract this API must serve**; see [Frontend Contract](#frontend-contract-future-api) below.

The architecture is cloned from the sibling project **`D:\My Career\ASP.NET\MyProjects\Travelling management`**. Onion layers, Generic Repository + Specification, Unit of Work, and claims-based RBAC are all ported from it. When a pattern here is unclear, that project's `CLAUDE.md` is the long-form reference.

| | |
|---|---|
| **Solution** | `maqar-properties-hub.sln` |
| **Framework** | `net9.0` |
| **DB** | SQL Server (`MaqarDB`) via EF Core 9 |
| **Auth (Admin)** | ASP.NET Identity, cookie-based (NOT JWT) |
| **Auth (Api)** | None yet |
| **Logging** | Serilog: console + daily rolling files in `<project>/Logs/` |
| **Mapping** | AutoMapper, Admin only (`AddAutoMapper(typeof(Program).Assembly)`) |
| **UI** | Skote Admin Template (Bootstrap 5.0.0-beta2, dark sidebar, Boxicons). English, LTR |

**UI preference:** modern, professional, premium.

---

## Architecture — Onion / Clean Architecture

```
Maqar.Domain       (Core — zero project references)
    ├── Authorization/     (Permissions, PermissionCatalog, PermissionClaims, AppRoles, AppClaimTypes)
    ├── Enums/             (Gender)
    ├── Models/            (BaseModel; Models/Identity/AppUser)
    ├── Repositories/      (IGenericRepository<T>)
    ├── Services/          (interfaces + DTOs only — OperationResult, ICurrentUserService,
    │                       IFileStorageService, Identity/IRoleService, Identity/IUserAccountService)
    ├── Specifications/    (ISpecifications<T>, BaseSpecifications<T>)
    └── IUnitOfWork.cs

Maqar.Repository   (Infrastructure — references Maqar.Domain)
    ├── Data/MaqarContext.cs               (IdentityDbContext<AppUser>)
    ├── Data/Configrations/                (Fluent API per entity — "Configrations" typo kept from Travel; don't rename)
    ├── Data/AppIdentityDbContextSeed.cs   (SuperAdmin role + MaqarAdmin user)
    ├── Data/Migrations/                   (InitialIdentity)
    ├── GenericRepository.cs
    ├── SpecificationEvalutor.cs           (typo intentional — keep it)
    ├── Storage/FileStorageService.cs      (IFileStorageService impl — disk-backed under wwwroot/Files)
    └── UnitOfWork.cs

Maqar.Services     (Business Logic — references Maqar.Domain)
    └── Identity/          (RoleService, UserAccountService)

Maqar.Admin        (MVC dashboard — references Maqar.Repository + Maqar.Services)
    ├── Authorization/     (PermissionRequirement, PermissionAuthorizationHandler,
    │                       PermissionPolicyProvider, AppUserClaimsPrincipalFactory)
    ├── Controllers/       (Account, Home, Role, User)
    ├── Helpers/           (AvatarHelper, PermissionIcons)
    ├── Services/          (CurrentUserService — web-coupled impls)
    ├── TagHelpers/        (PermissionTagHelper)
    ├── ViewModels/        (Login, Role, User, Error)
    ├── Views/             (Account, Home, Role, User, Shared layout partials)
    ├── wwwroot/back/assets/   (Skote: css, fonts, images, libs, js)
    ├── wwwroot/css/app-extras.css   (the ONLY project CSS)
    └── Program.cs

Maqar.Api          (Web API — references Maqar.Repository + Maqar.Services)
    ├── Controllers/       (empty — public endpoints go here)
    └── Program.cs         (Serilog, DbContext, UoW, generic repo, OpenAPI, CORS "Frontend")
```

### Dependency Flow

```
Maqar.Admin → Maqar.Services   → Maqar.Domain
Maqar.Admin → Maqar.Repository → Maqar.Domain
Maqar.Api   → Maqar.Services   → Maqar.Domain
Maqar.Api   → Maqar.Repository → Maqar.Domain
```

**Key rules:**
- `Maqar.Domain` has ZERO project references. Its only NuGet package is `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.
- Service **interfaces** live in `Maqar.Domain/Services/`. **Implementations** live in `Maqar.Services/`, or in the web project if they are web-coupled.
- Keep `Maqar.Services` and `Maqar.Repository` free of **web** types: no `IFormFile`, `HttpContext`, `SignInManager` or controllers. Both Admin **and** Api consume them. `UserManager`/`RoleManager` are fine.

---

## NuGet Packages by Project

| Project | Key Packages |
|---|---|
| **Maqar.Domain** | `Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.21` |
| **Maqar.Repository** | `Microsoft.EntityFrameworkCore.SqlServer 9.0.21` (+ ref Maqar.Domain) |
| **Maqar.Services** | _(no NuGet; refs Maqar.Domain only)_ |
| **Maqar.Admin** | `AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1`, `Serilog.AspNetCore 10.0.0`, `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation 9.0.21`, `Microsoft.EntityFrameworkCore.Tools 9.0.21` |
| **Maqar.Api** | `Microsoft.AspNetCore.OpenApi 9.0.6`, `Serilog.AspNetCore 10.0.0` |

Keep the EF/Identity packages on **one version** across projects (currently 9.0.21).

---

## Domain Models

- **`BaseModel`**: `int Id`. Every DB-persisted entity inherits from it, and that is what `IGenericRepository<T>` requires.
- **`AppUser : IdentityUser`**:
  - Properties: `FirstName`, `LastName`, `FullName` (computed), `Gender`, `IsActive` (default true), `ImageUrl?`.
  - Configured in `AppUserConfig`: FirstName/LastName required, max 100; ImageUrl max 500; unique indexes on UserName and Email.

No real-estate entities yet. See [Frontend Contract](#frontend-contract-future-api) for the planned ones.

| Enum | Values | Location |
|---|---|---|
| `Gender` | `Male=1`, `Female=2` | `Maqar.Domain/Enums/` |

---

## Key Patterns

### Generic Repository + Specification

All data access for `BaseModel` entities goes through `IGenericRepository<T>` + `ISpecifications<T>`:

```csharp
var spec = new PropertyWithDetailsSpec(id);
var one  = await _unitOfWork.Repository<Property>().GetByIdAsync(spec);
var many = await _unitOfWork.Repository<Property>().GetAllAsync(new PropertyFilterSpec(filters));
```

- **`IGenericRepository<T>`** has `GetAllAsync(spec)`, `GetByIdAsync(spec)` (FirstOrDefault over the spec), `AddAsync`, `Update` and `Delete`.
- **`ISpecifications<T>`** carries:
  - `Criteria` (WHERE)
  - `Includes` (expressions)
  - `IncludeStrings` (ThenInclude chains, e.g. `"Images"`)
  - `OrderBy` / `OrderByDesc`
  - `Skip` / `Take` / `IsPaginationEnabled`
- **`BaseSpecifications<T>`** has two constructors: parameterless (get all) and criteria. Helpers are `AddInclude(string)`, `SetOrderBy`, `SetOrderByDesc` and `ApplyPagination(skip, take)`.
- Name spec classes with the **`Spec`** suffix and put them in per-entity folders under `Maqar.Domain/Specifications/`.
- **`SpecificationEvalutor`** applies them in this order: Criteria → OrderBy → OrderByDesc → Skip/Take → Includes → IncludeStrings.

### Unit of Work + Transactions

- `_unitOfWork.Repository<Entity>()` returns a repository, cached per entity name in a `Hashtable`.
- `await _unitOfWork.CompleteAsync()` is a plain `SaveChangesAsync`.

Wrap **multiple** operations that must be atomic in a transaction:

```csharp
await _unitOfWork.BeginTransactionAsync();
try { /* ... several Repository + CompleteAsync calls ... */ await _unitOfWork.CommitTransactionAsync(); }
catch { await _unitOfWork.RollbackTransactionAsync(); throw; }
```

- A single `CompleteAsync()` is already atomic and needs no transaction.
- `DisposeAsync` rolls back any open transaction.

> **No DuplicateCode / code-generation mechanism.** Travelling management's `UnitOfWork` turns unique-index violations on `_Code` columns into a `DuplicateCodeException` and retries. That was deliberately **not** ported. `CompleteAsync` lets `DbUpdateException` surface as-is. If listings ever need sequential reference codes, design that fresh.

### OperationResult

Services return **`OperationResult` / `OperationResult<T>`** (`Maqar.Domain/Services/OperationResult.cs`), and `From(IdentityResult)` converts Identity results. Controllers turn a failure into `ModelState` errors (forms) or `{ success, message }` JSON (AJAX).

---

## Service Layer

| Interface (Maqar.Domain/Services) | Implementation | Purpose |
|---|---|---|
| `IFileStorageService` | `FileStorageService` (Maqar.Repository/Storage) | Disk-backed upload/delete under `wwwroot/Files/{folder}`; takes a `Stream`, not `IFormFile` |
| `ICurrentUserService` | `CurrentUserService` (Maqar.Admin/Services) | Current user id + username |
| `IRoleService` | `RoleService` (Maqar.Services/Identity) | Role CRUD + atomic role-claim replacement |
| `IUserAccountService` | `UserAccountService` (Maqar.Services/Identity) | User CRUD, roles, direct permissions, lockout guards |

Only `Maqar.Admin` registers these today. `Maqar.Api` registers just the DbContext, `IUnitOfWork` and the generic repository; add services there as endpoints need them. Note that the Identity services need `AddIdentityCore<AppUser>()` in the Api first.

---

## Database

- **`MaqarContext : IdentityDbContext<AppUser>`**, configured through `ApplyConfigurationsFromAssembly`.
- When you add an entity: add a `DbSet`, a `{Entity}Config` in `Data/Configrations/`, and a migration.

**Migrations** (run from the solution root):
```
dotnet ef migrations add <Name> -p Maqar.Repository -s Maqar.Admin -o Data/Migrations
```

- **`Maqar.Admin` applies migrations on startup** (`MigrateAsync`). `Maqar.Api` never migrates.
- **Rebuild before `dotnet run --no-build`**, or the new migration isn't in the DLL (see Gotchas).

**Connection string** (`appsettings.json` → `DefaultConnection`, the same in both web projects):
```
Server=.;Database=MaqarDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

**Guardrails:**
- **SQL Server cascade cycles (error 1785):** use `Cascade` only for ownership FKs (e.g. Property → PropertyImages) and `NoAction` for reference FKs (e.g. Property → Area).
- **Nullable columns in a unique index need `.HasFilter("[Col] IS NOT NULL")`.** SQL Server treats NULLs as equal.
- **Don't `HasDefaultValue(1)` on an enum property.** Initialise the default in C# instead.
- **`sqlcmd` deletes on Identity tables need `-I`** (QUOTED_IDENTIFIER ON) because of the filtered unique indexes.

---

## Program.cs

### `Maqar.Admin` — flat `#region` style, like Travel

**Services, in registration order:**
1. Serilog.
2. AutoMapper.
3. `IUnitOfWork` and `IFileStorageService` (factory with `WebRootPath`).
4. `HttpContextAccessor` and `ICurrentUserService`.
5. MVC with runtime compilation.
6. `AddDbContext<MaqarContext>` (SqlServer).
7. Open-generic `IGenericRepository<>`.
8. `AddIdentity<AppUser, IdentityRole>`:
   - Password policy: digit, upper, lower, non-alphanumeric, minimum length 8.
   - Unique email.
   - Chained with `.AddEntityFrameworkStores<MaqarContext>().AddDefaultTokenProviders().AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()`.
9. The **Authorization** region:
   - `AddAuthorization`.
   - The policy provider as a **singleton** and the handler as **scoped**.
   - `SecurityStampValidatorOptions.ValidationInterval = 2 min`.
   - Role and user services.
10. The cookie: `/Account/Login`, `/Account/AccessDenied`, 8h sliding expiry.

**Startup:**
1. `MigrateAsync`.
2. `PermissionCatalog.FindCatalogGaps()`, logging a warning if it finds any.
3. `SeedRolesAsync`, then `SeedUserAsync`, then `AssignSuperAdminAsync`.

**Pipeline:** `UseStaticFiles` (needed for runtime uploads) → routing → authentication → authorization → `MapStaticAssets` → default route `{controller=Account}/{action=Login}/{id?}`.

### `Maqar.Api`

- Serilog, controllers, OpenAPI (`/openapi/v1.json` in Development), `MaqarContext`, `IUnitOfWork` and the generic repo.
- CORS policy **`Frontend`**, with origins from `Cors:AllowedOrigins` in `appsettings.json`. Defaults: `http://localhost:8080` (Lovable/Vite dev server) and `http://localhost:5173`.
- No auth.

---

## Authorization System (RBAC — ported from Travel)

Claims-based permissions on top of ASP.NET Identity. **There is no Permission table and no migration.** The catalog is C# constants, and assignments are rows in `AspNetRoleClaims` / `AspNetUserClaims` with `ClaimType = "permission"`.

### `Maqar.Domain/Authorization/`

| File | Purpose |
|---|---|
| `AppClaimTypes.cs` | `Permission = "permission"` |
| `Permissions.cs` | Nested static classes per module, with `const string` values shaped `Module.Action`. **Consts are mandatory**, because `[Authorize(Policy = ...)]` needs a compile-time constant |
| `PermissionCatalog.cs` | Display metadata per module (key, name, order, descriptors). Drives the picker. Provides `IsValid`, `Sanitize`, `DisplayName`, `ModuleOf` and `FindCatalogGaps()` |
| `PermissionClaims.cs` | `Pack` / `Unpack`. The cookie carries **one** comma-joined permission claim; the DB keeps one row each |
| `AppRoles.cs` | `SuperAdmin` + `IsProtected(name)`. Named `AppRoles` so it doesn't clash with `Permissions.Roles` |

**Current modules (13 permissions):**
- **Users** (Order 1): `View`, `Create`, `Edit`, `Delete`, `ToggleActive`, `ResetPassword`, `AssignRoles`, `ManagePermissions`.
- **Roles** (Order 2): `View`, `Create`, `Edit`, `Delete`, `ManagePermissions`.

**Adding a module** (do this for Properties, Areas, Features, ContactMessages):
1. Add a nested class of consts to `Permissions.cs`.
2. Add a matching `new("Key", "Display", Order, [...])` entry to `PermissionCatalog.Modules`. Append it; inserting mid-list means renumbering every later `Order`.
3. Add an icon to `Maqar.Admin/Helpers/PermissionIcons.cs`, keyed by the same module `Key`.
4. Gate controller actions with `[Authorize(Policy = Permissions.X.Y)]`, keeping class-level `[Authorize]`.
5. Add sidebar items in `_Sidebar.cshtml`, each wrapped in `<permission required="...">`, with a `mode="any"` wrapper around the section heading.

`FindCatalogGaps()` logs a Serilog warning at startup if the constants and the catalog drift apart.

### `Maqar.Admin/Authorization/`

- **`PermissionAuthorizationHandler`**: **SuperAdmin short-circuits**. Everyone else needs the permission in the unpacked claim.
- **`PermissionPolicyProvider`** (singleton): builds and caches a policy for any `PermissionCatalog.IsValid` name. Anything else falls through to the default provider, so a typo fails loudly.
- **`AppUserClaimsPrincipalFactory`**: the base factory already unions role and user claims. This one de-duplicates them, drops unknown ones, packs the survivors into one claim, and emits **none** for SuperAdmin.

### Effective permissions, propagation, UI

- **Effective = role permissions ∪ direct user permissions.** Grants only; there is no deny.
- On `/User/Permissions/{id}`, role-inherited permissions render as `checked disabled`. Disabled inputs don't post, so they can't be saved as direct grants.
- **Propagation:** the 2-minute `SecurityStampValidator` tick re-runs the claims factory, so permission changes reach signed-in users without a re-login. Rotating the stamp (role change, deactivation, password reset) signs the user out.
- **UI gating:** `<permission required="@Permissions.Users.View" mode="any|all">…</permission>`.
  - It evaluates through `IAuthorizationService`, never `User.HasClaim`, because SuperAdmin carries no claims.
  - `output.TagName = null` erases the wrapper, so `<li>` stays a direct child of `<ul class="metismenu">`.
  - It's C#, so after changing it you must **rebuild**; Razor runtime compilation won't pick it up.
- **Action naming:** `UserController.DirectPermissions` carries `[ActionName("Permissions")]`. A method named `Permissions` would shadow the `Permissions` class.

### Lockout guards

- `UserAccountService` blocks deleting or deactivating yourself, removing your own SuperAdmin role, and anything that would leave zero active SuperAdmins.
- `RoleService` blocks editing, renaming or deleting SuperAdmin, and deleting any role that still has members.

### Seeding (`Maqar.Repository/Data/AppIdentityDbContextSeed.cs`)

Each step has its own guard:
1. `SeedRolesAsync` creates SuperAdmin with **no claims**.
2. `SeedUserAsync` runs only when there are no users. It creates **`MaqarAdmin` / `admin@maqar.com` / `Admin@123`**. These credentials are hardcoded, mirroring Travel. **Change them before any shared or production deploy.**
3. `AssignSuperAdminAsync` makes sure somebody holds SuperAdmin.

### Testing RBAC

- **SuperAdmin proves nothing.** It bypasses the handler. Test with a plain user in a limited role.
- Verified on 2026-10-16 with a `Viewer` role holding only `Users.View`:
  - `/User/Index` returned 200.
  - `/Role/Index` and `/User/Create` redirected to `/Account/AccessDenied`.
  - The sidebar showed Users but not Roles.

---

## ViewModels (Maqar.Admin/ViewModels)

- `LoginViewModel`: `EmailOrUserName`, `Password`, `RememberMe`.
- `ErrorViewModel`: in namespace `Maqar.Admin.Models`, kept from Travel.
- `RoleViewModel` + `PermissionGroupViewModel` / `PermissionItemViewModel`. The item carries `IsSelected`, `IsLocked`, `LockedReason` and `IsAlsoDirect`, so `_PermissionModules.cshtml` serves both roles and users.
- `UserFormViewModel`, `RoleOptionViewModel`, `UserPermissionsViewModel`, `ResetPasswordViewModel`.

Rules:
- Use the **`ViewModel`** suffix, and `[Remote]` for uniqueness checks.
- **Checkbox lists** (`SelectedPermissions`, `SelectedRoles`) bind from repeated `name="X"` inputs. **Never put `asp-for` on them**: it adds a hidden `false` input that corrupts the list. Initialise them to `new()`.

---

## Frontend Contract (future API)

From `D:\My Career\maqar-properties-hub\src\data\properties.ts`. Nothing below is implemented yet.

- **Property:**
  - `id`, `type` (`apartment|villa|shop|land|building`), `listingType` (`sale|rent`).
  - `rentalUnit?` (`whole|room|bed`, rent only). For room or bed, the specs describe that room.
  - `area` (key), `featured`, `price` (EGP; the total for sale, monthly for rent).
  - `bedrooms`, `bathrooms`, `size` (m²), `finishing` (`superLux|lux|semiFinished|core`).
  - `createdAt` (`yyyy-MM-dd`), `images[]`, `features[]`.
  - `title` / `location` / `description` as **`Localized { ar, en }`**.
- **Lookups:**
  - Areas: `fifthSettlement|maadi|sheikhZayed|nasrCity|october`.
  - Features: `elevator|security|garage|garden|ac|pool|balcony|kitchen`.
  - **Return keys, not display text.** The frontend translates them.
- **Listing query params:** `area`, `type`, `listingType`, `rentalUnit`, `minPrice`, `maxPrice`, `minSize`, `maxSize`, `bedrooms`, `page`.
- **Sorting:** `newest` | `asc` | `desc` (by price). Page size is 6.
- **Endpoints the site expects:**
  - `GET /api/properties` (filter, sort, page)
  - `GET /api/properties/{id}`
  - featured listings
  - area/feature lookups
  - `POST /api/contact` (name, email, phone, message)
- **Likely entities:** `Property`, `Area`, `Feature` (many-to-many with Property), `PropertyImage` (Property owns many), `ContactMessage`.
  - Store the localized text as `TitleAr`/`TitleEn`-style column pairs, or as an owned type, and map them to `{ ar, en }` in DTOs.

---

## Coding Conventions

1. **Casing:** **PascalCase** for public members and types. **_camelCase** for injected dependencies (`_unitOfWork`, `_mapper`).
2. **Suffixes:** `Spec`, `SpecParams`, `ViewModel`, `Dto`, `Config` (EF), `Profile` (AutoMapper).
3. **Placement:** interfaces in `Maqar.Domain/Services/`, implementations in `Maqar.Services/`. The repo interface is in Domain and its implementation in Repository. Enums go in `Maqar.Domain/Enums/`.
4. **Entities:** use `DateTime.UtcNow` for timestamps. All entities inherit `BaseModel`.
5. **Comments:** only when the intent isn't obvious.
6. **Controller attributes:**
   - `[Authorize(Policy = ...)]` for permission gates.
   - `[Authorize]` for authenticated-only.
   - `[AllowAnonymous]` for login, access-denied and `/Home/Error`.
7. **Admin controller responses:** Delete returns JSON for AJAX. Create/Edit are POST + redirect, with Toastr messages via `TempData`.
8. **API controllers:** return DTOs, never entities. Keep the keys and `Localized` shape the frontend expects.
9. **Thin controllers:** keep them HTTP-only and push orchestration into services.

---

## UI/UX Guidelines (Maqar.Admin)

- **Template:** Skote (Bootstrap 5), `data-sidebar="dark"`, Boxicons.
- **Notifications:** Toastr via `TempData` (`_Notifications.cshtml`). Toastr loads from a CDN.
- **Tables:** DataTables.
- **Delete flow:** SweetAlert2 confirm → AJAX POST → Toastr.
- **Forms:** card forms with jquery validate + unobtrusive validation.
- **Layouts:** `_AuthLayout` for login (no sidebar) and `_Layout` for the admin pages (`_Head`, `_Nav`, `_Sidebar`, `_Footer`, `_RightSideBar`, `_Scripts`).
- **Branding** (copied from the frontend's `src/assets`), in `wwwroot/back/assets/images/`:
  - `Logo_Maqar.png`: light backgrounds. Used on login, the dashboard and the topbar `logo-dark`.
  - `Logo_Maqar_Dark.png`: the dark sidebar's `logo-light`.
  - `Logo_Maqar_Icon.png`: the collapsed sidebar.
  - `favicon.png`.
- **Copied Skote JS:** Skote's `app.js` and its demo `js/pages/` came over from Travel, plus the project scripts `role-permissions.js`, `user-form.js` and `notifications.js`. Travel's domain scripts (booking, package, etc.) were not copied.

### ⚠️ Bootstrap **5.0.0-beta2**, not 5.3

**Check that a class exists** in `back/assets/css/bootstrap.min.css` before using it. Missing in this build:
- `bg-*-subtle`, `text-*-emphasis`, `border-*-subtle`
- the `--bs-*-rgb` variables
- `btn-soft-*`

`bg-*-subtle` and `btn-soft-*` are shimmed in `app-extras.css`.

**Palette:** primary `#556ee6`, secondary `#74788d`, success `#34c38f`, info `#50a5f1`, warning `#f1b44c`, danger `#f46a6a`, dark `#343a40`.

### `wwwroot/css/app-extras.css`: the only place for project CSS

- Never edit the Skote vendor CSS.
- It is linked last, **without an `id`**, in both `_Head` and `_AuthLayout`. `app.js` switches light/dark by rewriting the `href` of `#bootstrap-style` / `#app-style`.
- New components must be theme-agnostic: use `var(--bs-*)` hues and `rgba()` neutrals, never a literal `#fff`.

### Boxicons

Verify a glyph with `grep "\.bx-name:before" wwwroot/back/assets/css/icons.min.css` before using it. A missing icon renders blank without any error. Many glyphs exist only as `bxs-`.

### 🚫 No Bootstrap tooltips

They render detached in this template; Travel tried and abandoned them. Show the text inline instead, as muted text (`.perm-desc` / `.perm-source`) or badges.

---

## Important Notes

- **`SpecificationEvalutor.cs`** and **`Data/Configrations/`** are intentional typos. Do NOT rename them.
- **The default route action is `Login`**, so `/User` and `/Role` resolve to `User/Login` and return 404. Link with `asp-action="Index"`, as the sidebar does.
- **`Account/Profile` was not ported.** Travel's `_Nav` linked to a commented-out action, so the link was removed here. Reviving it needs a `ProfileViewModel` + view.
- **Runtime uploads** go to `Maqar.Admin/wwwroot/Files/`, which is gitignored and served through `UseStaticFiles`.
- **`Logs/`** folders are gitignored.
- **No `appsettings.Production.json` exists.** Unlike Travel, a bare `dotnet run` can only hit the local `MaqarDB`.
- **`.claude/commands/commit.md`** is the `/commit` workflow copied from Travel.

---

## Gotchas & Past Mistakes for Claude

> This dated log is the version history of this file. Add an entry for every mistake or trap so it is never repeated.

### 2026-10-16 — v1: architecture cloned from Travelling management
- **The original skeleton had four bugs from a partial copy:**
  - `GenericRepository<T>` didn't declare `: IGenericRepository<T>`, so the `as` cast in `UnitOfWork.Repository<T>()` silently returned **null**.
  - `GenericRepository.cs` was missing `using Microsoft.EntityFrameworkCore;` (`ToListAsync`).
  - `MaqarContext` didn't call `: base(options)`, so the provider was never configured.
  - `IGenericRepository.cs` was missing `using Maqar.Domain.Specifications;`.
  - All four are fixed. **When copying files from Travel, build immediately.** A missing interface declaration compiles fine and fails only at runtime.
- **DuplicateCode handling was removed on purpose.** `UnitOfWork.IsCodeUniqueViolation` referenced Travel's `IX_Bookings_Code` / `IX_Packages_Name`. Don't re-add it.
- **`dotnet run --no-build` right after `dotnet ef migrations add` runs the OLD DLL.** The migration isn't compiled in, `MigrateAsync` applies nothing, and seeding fails with `Invalid object name 'AspNetRoles'`. Build first.
- **A running `Maqar.Admin` locks `bin/`,** so the next build fails with MSB3027. Stop the process first: `Get-Process Maqar.Admin | Stop-Process`.
- **A bulk `Travel.` → `Maqar.` rename also hit asset names.** `Logo_MustaTravel.png` became `Logo_MustaMaqar.png`, which doesn't exist. After any bulk rename, grep for file references.

### Reusable guardrails (apply as the domain is built)
- **Roles and users can't go through `IUnitOfWork.Repository<T>()`.** It requires `BaseModel`, which has an int Id, and Identity uses string Ids. Use `UserManager`/`RoleManager`. They share the scoped `MaqarContext`, so `BeginTransactionAsync` still makes multi-claim changes atomic.
- **Go through `RoleExistsAsync`/`FindByNameAsync`/`SetRoleNameAsync`.** Adding an `IdentityRole` to the context directly leaves `NormalizedName` null.
- **Seed guards must be independent.** `if (!Users.Any())` is false on any existing DB.
- **`AccessDeniedPath` needs its view to exist** before you add policy gates.
- **Gate `IsNameAvailable`-style `[Remote]` endpoints on `View`, not `Create`.** The Edit form calls them too.
- **Kestrel's default body limit (~30 MB) kills multi-image uploads** before model binding runs. Property image uploads will need `[RequestSizeLimit]` + `[RequestFormLimits(MultipartBodyLengthLimit = ...)]`.
- **Submit-button spinners:** check `if (!$(form).valid()) return;` *before* disabling the button.
- **CRLF vs LF:** if a targeted string replace silently fails to match, re-read the file and overwrite it whole.
