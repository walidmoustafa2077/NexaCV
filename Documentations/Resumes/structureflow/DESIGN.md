---
name: StructureFlow
colors:
  surface: '#f9f9ff'
  surface-dim: '#d9d9e0'
  surface-bright: '#f9f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f3f3fa'
  surface-container: '#ededf4'
  surface-container-high: '#e8e7ee'
  surface-container-highest: '#e2e2e9'
  on-surface: '#1a1b20'
  on-surface-variant: '#444651'
  inverse-surface: '#2f3035'
  inverse-on-surface: '#f0f0f7'
  outline: '#747782'
  outline-variant: '#c4c6d2'
  surface-tint: '#3d5ca4'
  primary: '#3a59a1'
  on-primary: '#ffffff'
  primary-container: '#5472bc'
  on-primary-container: '#fefcff'
  inverse-primary: '#b1c5ff'
  secondary: '#545e7c'
  on-secondary: '#ffffff'
  secondary-container: '#cfd9fd'
  on-secondary-container: '#545e7d'
  tertiary: '#864f02'
  on-tertiary: '#ffffff'
  tertiary-container: '#a4671e'
  on-tertiary-container: '#fffbff'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#dae2ff'
  primary-fixed-dim: '#b1c5ff'
  on-primary-fixed: '#001947'
  on-primary-fixed-variant: '#22438a'
  secondary-fixed: '#dae2ff'
  secondary-fixed-dim: '#bcc6e9'
  on-secondary-fixed: '#101a36'
  on-secondary-fixed-variant: '#3c4663'
  tertiary-fixed: '#ffdcbd'
  tertiary-fixed-dim: '#ffb86e'
  on-tertiary-fixed: '#2c1600'
  on-tertiary-fixed-variant: '#693c00'
  background: '#f9f9ff'
  on-background: '#1a1b20'
  surface-variant: '#e2e2e9'
typography:
  h1:
    fontFamily: Manrope
    fontSize: 30px
    fontWeight: '700'
    lineHeight: 36px
    letterSpacing: -0.02em
  h2:
    fontFamily: Manrope
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
    letterSpacing: -0.01em
  body-base:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  body-sm:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  label-caps:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.05em
  input-text:
    fontFamily: Inter
    fontSize: 15px
    fontWeight: '400'
    lineHeight: 24px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  base-unit: 4px
  container-max-width: 768px
  form-gap: 24px
  section-margin: 48px
  input-padding-x: 12px
  input-padding-y: 10px
---

# Product Requirements Document: StructureFlow

## Project Overview
A minimalist, professional SaaS platform focused on structured data entry (Resumes, Menus, etc.) using a "Form-Only" approach.

## Tech Stack
- **Framework:** React
- **Styling:** Tailwind CSS
- **Components:** Shadcn/UI
- **Icons:** Lucide-react

## Design Philosophy
- **Minimalist & Professional:** Clean layouts, generous whitespace, and a focus on content. The interface uses a periwinkle and slate blue palette to provide a sense of calm reliability.
- **Approachably Modern:** High-trust interface utilizing rounded shapes (0.5rem base) and soft edges to make data entry feel contemporary and less rigid.
- **Strictly Form-Based:** No "canvas" or drag-and-drop editing. All inputs via structured, stepped forms, utilizing a centered 768px layout for maximum focus.
- **Responsive & Accessible:** Following best practices for all devices and users, with high-contrast typography in Inter and Manrope.

## Core Screens to Build
1. **Dashboard:** Overview of user projects/forms, featuring subtle card-based layouts.
2. **Stepped Form Entry:** The primary data entry interface with clear typography and consistent spacing.
3. **Template Gallery:** Selection of professional output styles presented in a clean grid.
4. **Data Preview/Submission:** Reviewing the entered data before finalization, using tonal layers to separate data sections.