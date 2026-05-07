---
name: nexacv-add-frontend-feature
description: 'Add a new frontend page or feature to NexaCV. Use when adding a route, page, data-fetching hook, or API client function. Covers API function → query key → React Query hook → Next.js App Router page → Shadcn/UI + Tailwind components. Triggers: "add page", "new frontend feature", "add route", "new screen", "add component", "add hook".'
argument-hint: 'Describe the feature (e.g. "page to list user activity")'
---

# NexaCV: Add Frontend Feature

Covers the full lifecycle for adding a new frontend feature following NexaCV's Next.js 16 + React Query conventions.

## Stack Reference
- **Router**: Next.js App Router (`src/app/`) with route groups: `(auth)`, `(main)`, `(public)`, `(wizard)`
- **Server state**: TanStack React Query — `useQuery` / `useMutation`
- **Global state**: Zustand (auth token, wizard form data) in `src/store/`
- **Forms**: React Hook Form + Zod for validation
- **UI**: Shadcn/UI components + Tailwind CSS + Material Icons
- **API client**: `apiFetch` from `src/lib/api/client.ts` (auto-injects JWT Bearer token)
- **Types**: Shared API types in `src/types/api.types.ts`

---

## Step 1 — TypeScript Types

Add request/response types to `frontend/src/types/api.types.ts` if not already present.

```ts
// Add to api.types.ts
export interface MyFeatureDto {
  id: string;
  name: string;
  createdAt: string;
}

export interface CreateMyFeatureRequest {
  name: string;
}
```

Keep types aligned with the backend DTO field names (camelCase on the wire).

---

## Step 2 — API Client Function

Add a function to the appropriate file in `frontend/src/lib/api/` (or create a new file for a new domain).

```ts
// frontend/src/lib/api/myfeature.ts
import { apiFetch } from "./client";
import type { MyFeatureDto, CreateMyFeatureRequest } from "@/types/api.types";

export function getMyFeatures(): Promise<MyFeatureDto[]> {
  return apiFetch<MyFeatureDto[]>("/api/myfeature");
}

export function createMyFeature(data: CreateMyFeatureRequest): Promise<MyFeatureDto> {
  return apiFetch<MyFeatureDto>("/api/myfeature", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export function deleteMyFeature(id: string): Promise<void> {
  return apiFetch<void>(`/api/myfeature/${id}`, { method: "DELETE" });
}
```

`apiFetch` handles:
- Prepending the base URL (`NEXT_PUBLIC_API_URL`)
- Injecting the JWT `Authorization: Bearer <token>` header
- Throwing `ApiError` (≥ 400) or `ValidationError` (422)

---

## Step 3 — Query Keys

Add new keys to `frontend/src/lib/query/keys.ts`:

```ts
export const queryKeys = {
  // ... existing keys
  myFeatures: () => ["myfeature"] as const,
  myFeature: (id: string) => ["myfeature", id] as const,
} as const;
```

---

## Step 4 — React Query Hook

Create `frontend/src/hooks/useMyFeature.ts`:

```ts
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { getMyFeatures, createMyFeature, deleteMyFeature } from "@/lib/api/myfeature";
import { queryKeys } from "@/lib/query/keys";

export function useMyFeatures() {
  return useQuery({
    queryKey: queryKeys.myFeatures(),
    queryFn: getMyFeatures,
  });
}

export function useCreateMyFeature() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: createMyFeature,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.myFeatures() });
    },
  });
}

export function useDeleteMyFeature() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: deleteMyFeature,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.myFeatures() });
    },
  });
}
```

---

## Step 5 — Next.js Route

Create the page folder and file under the correct route group:

| Group | Path prefix | Use for |
|-------|-------------|---------|
| `(auth)` | `/sign-in`, `/sign-up` | Unauthenticated pages |
| `(main)` | `/dashboard`, `/resumes`, `/settings` | Authenticated app pages |
| `(public)` | `/`, `/about` | Public marketing pages |
| `(wizard)` | `/create/*` | Multi-step resume wizard |

```
frontend/src/app/(main)/myfeature/
├── page.tsx          # Main page
└── [id]/
    └── page.tsx      # Detail page
```

---

## Step 6 — Page Component

```tsx
// frontend/src/app/(main)/myfeature/page.tsx
"use client";

import { toast } from "sonner";
import { useMyFeatures, useDeleteMyFeature } from "@/hooks/useMyFeature";
import { SkeletonCard } from "@/components/shared/SkeletonCard";

export default function MyFeaturePage() {
  const { data: items, isLoading } = useMyFeatures();
  const { mutate: deleteItem } = useDeleteMyFeature();

  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {Array.from({ length: 4 }).map((_, i) => (
          <SkeletonCard key={i} className="h-40" />
        ))}
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {(items ?? []).map((item) => (
        <div key={item.id} className="rounded-xl border border-outline-variant p-4">
          <p className="font-h3 text-h3 text-on-surface">{item.name}</p>
          <button
            onClick={() => {
              deleteItem(item.id, {
                onSuccess: () => toast.success("Deleted."),
                onError: () => toast.error("Failed to delete."),
              });
            }}
            className="text-error hover:underline mt-2"
          >
            Delete
          </button>
        </div>
      ))}
    </div>
  );
}
```

**Conventions**:
- All pages with client state use `"use client"` directive.
- Use `toast.success` / `toast.error` from `sonner` for feedback.
- Use `SkeletonCard` for loading placeholders.
- Follow Tailwind classes from the design system: `text-on-surface`, `border-outline-variant`, `bg-surface-container`, `text-primary`, etc.

---

## Step 7 — Form with Validation (if the page has a form)

```tsx
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";

const schema = z.object({
  name: z.string().min(1, "Name is required").max(200),
});
type FormData = z.infer<typeof schema>;

export function MyFeatureForm({ onSubmit }: { onSubmit: (data: FormData) => void }) {
  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <input {...register("name")} placeholder="Name" />
      {errors.name && <p className="text-error text-sm">{errors.name.message}</p>}
      <button type="submit">Submit</button>
    </form>
  );
}
```

---

## Checklist

- [ ] Types added to `src/types/api.types.ts`
- [ ] API client function added in `src/lib/api/`
- [ ] Query key added to `src/lib/query/keys.ts`
- [ ] React Query hook created in `src/hooks/`
- [ ] Route folder created under correct App Router group
- [ ] Page uses `"use client"` if it has state/interactivity
- [ ] Loading state shown with `SkeletonCard`
- [ ] Errors surfaced with `toast.error`
- [ ] Form uses React Hook Form + Zod (if applicable)
- [ ] No direct `fetch()` calls — always use `apiFetch` via the API module
