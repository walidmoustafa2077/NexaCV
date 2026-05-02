# NexaCV Design System Documentation

## 1. Core Principles
*   **Minimalism:** Use whitespace as a layout element, not just an empty space.
*   **Form-First:** Every interaction is data-driven. Inputs are the "Hero" components.
*   **Predictability:** Buttons, inputs, and feedback messages behave the same across all apps (CV, Menu, Recruiter).

---

## 2. Visual Foundation

### Color Palette
*   **Primary (Brand Blue):** `#57758F` (Used for Primary Buttons, Active States, Steppers)
*   **Secondary (Slate):** `#6C789B` (Used for Secondary buttons, Borders, Text accents)
*   **Tertiary (Accent/Action):** `#844D00` (Used for Highlights, AI-Action buttons, Success highlights)
*   **Neutral (System):** `#767770` (Used for Text, Input placeholders, background shades)

### Typography
*   **Headline Font:** `Manrope` (Clean, modern, highly legible at large sizes)
*   **Body & Label Font:** `Inter` (The gold standard for UI readability; excellent at small sizes)

---

## 3. Component Library & Tech Stack

### Frontend Core
*   **Framework:** React (Latest)
*   **Meta-Framework:** Next.js (Optimized for SEO/Hosting for Menus)
*   **Styling:** Tailwind CSS (utility-first)
*   **UI Components:** **Shadcn/UI** (Customizable, accessible, and clean)
*   **Icons:** `Lucide-React` (Matches the clean aesthetic of the UI)

### State & Data
*   **Forms:** `React Hook Form` (Mandatory for handling multi-step resume/menu forms)
*   **Schema Validation:** `Zod` (Shared validation between frontend and backend)
*   **Data Fetching:** `TanStack Query (React Query)` (Cached API state)
*   **Global State:** `Zustand` (Lightweight for session/auth state)

---

## 4. UI Patterns & Behavior

### Buttons
*   **Primary:** Solid Brand Blue (`#57758F`). Used for "Next Step," "Save," "Checkout."
*   **Secondary:** White background, thin border. Used for "Back," "Discard," "Cancel."
*   **Inverted:** Dark Slate/Neutral. Used for "Action" calls or deep focus.
*   **Outlined:** Transparent background, primary border. Used for subtle actions.

### Input Fields & Forms
*   **Border:** Neutral gray focus, Primary blue on active/interaction.
*   **Rounding:** `rounded-lg` (Matches the modern "soft" corporate look).
*   **AI Action:** Always place a "sparkle" icon (`Lucide-Sparkle`) near textareas that support AI refinement.

### Layout
*   **Container:** "Form Card" layout. All inputs must be contained in `bg-white` cards with `shadow-sm`.
*   **Spacing:** Consistent use of `gap-4` to `gap-6` across steps.
*   **Steppers:** Always maintain a progress indicator (Stepper) at the top for wizards to keep users focused.

---

## 5. Global Asset Strategy

*   **Icons:** Use `Lucide-React`.
    *   *System icons:* `Home`, `Search`, `User`, `Settings`.
    *   *Resume specific:* `FileText`, `GraduationCap`, `Briefcase`, `Sparkles`.
*   **Loading States:** Use **Skeleton screens** for data-fetching. Never use static loaders; skeletons improve perceived performance.
*   **Feedback/Notifications:** Use `Sonner` (for toast notifications). It is elegant and integrates perfectly with Shadcn/UI.

---

## 6. Maintenance Guide (Rules for Additions)
1.  **Never hardcode colors.** Always define them in `tailwind.config.js` (e.g., `primary: 'var(--color-primary)'`).
2.  **Naming:** If adding a component, name it based on its *function* (e.g., `RegenerationModal.tsx`) rather than its visual style.
3.  **Accessibility:** All inputs must have associated `Label` components. No "Floating Labels" that disappear.
4.  **Responsiveness:** Mobile-first. All forms must be single-column on mobile and use responsive grids (e.g., `grid-cols-1 md:grid-cols-2`) for larger screens.

---

### How to use this documentation:
*   **The "One-Look" Rule:** Any developer joining your project should be able to open this page and know how to style a "Primary Button" in under 5 seconds.
*   **Future Growth:** As you move to your "POS App," you will add a `POS-Specific` section, but you will keep the `Colors` and `Typography` the same, ensuring the NexaCV "family" feel.

**Does this structure cover everything you need for your documentation?** You can save this as a `DESIGN.md` in the root of your frontend repositories.