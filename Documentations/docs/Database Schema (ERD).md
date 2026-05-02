### 1. Entity-Relationship Diagram (ERD) Overview

*   **Users** (1) ---- (M) **User_Movements** (Tracks Logins/Updates)
*   **Users** (1) ---- (M) **Resumes** (A user can create multiple resumes)
*   **Templates** (1) ---- (M) **Resumes** (A template applies to many resumes)
*   **Resumes** (1) ---- (M) **Regenerations** (Tracks AI prompts and limits)
*   **Resumes** (1) ---- (1) **Transactions** (Payment for the final document)

---

### 2. Database Tables & Definitions

#### 2.1. `users`
Stores core user information. Passwords must be hashed via .NET Identity or Bcrypt.

| Column Name | Data Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | UUID | PRIMARY KEY | Unique identifier. |
| `first_name` | VARCHAR(50) | NOT NULL | User's first name. |
| `last_name` | VARCHAR(50) | NOT NULL | User's last name. |
| `username` | VARCHAR(50) | UNIQUE, NOT NULL | Chosen username. |
| `email` | VARCHAR(150) | UNIQUE, NOT NULL | User email address. |
| `password_hash` | VARCHAR(255) | NOT NULL | Securely hashed password. |
| `date_of_birth` | DATE | NULL | Optional or required based on UX. |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | Account creation time. |
| `last_login` | TIMESTAMPTZ | NULL | Updated automatically on login. |

#### 2.2. `user_movements` (Audit / Activity Log)
Records critical account actions for security and analytics.

| Column Name | Data Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | UUID | PRIMARY KEY | Unique log identifier. |
| `user_id` | UUID | FK -> `users(id)` | Which user performed the action. |
| `action_type` | VARCHAR(50) | NOT NULL | e.g., `LOGIN`, `PASSWORD_UPDATED`, `LOGOUT`. |
| `ip_address` | VARCHAR(45) | NULL | IPv4 or IPv6 address. |
| `user_agent` | TEXT | NULL | Browser/Device information. |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | Exact time of the action. |

#### 2.3. `templates`
Stores available templates and their base pricing.

| Column Name | Data Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | SERIAL | PRIMARY KEY | Standard auto-increment ID. |
| `name` | VARCHAR(100) | NOT NULL | e.g., "Modern Minimalist". |
| `industry_category` | VARCHAR(50) | NULL | e.g., "Corporate", "Creative". |
| `base_price_egp` | DECIMAL(10,2) | NOT NULL | e.g., 120.00 |
| `base_price_usd` | DECIMAL(10,2) | NOT NULL | e.g., 3.00 |
| `supports_word` | BOOLEAN | DEFAULT FALSE | Can this be downloaded as DOCX? |
| `is_active` | BOOLEAN | DEFAULT TRUE | Can users currently select this? |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | - |

#### 2.4. `resumes`
The core entity holding the user's progress. We use `JSONB` for `raw_data` and `final_data` to store dynamic form fields (Education array, Experience array, Skills) without needing dozens of JOIN tables.

| Column Name | Data Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | UUID | PRIMARY KEY | Secure unique ID for sharing/preview. |
| `user_id` | UUID | FK -> `users(id)` | Owner of the resume. |
| `template_id` | INT | FK -> `templates(id)` | Chosen template. |
| `status` | VARCHAR(20) | NOT NULL | `DRAFT`, `COMPLETED`, `PAID`. |
| `raw_data` | JSONB | NULL | Data exactly as entered in the form. |
| `final_data` | JSONB | NULL | The AI-polished version shown in the editor. |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | - |
| `updated_at` | TIMESTAMPTZ | DEFAULT NOW() | Updated on every autosave. |

#### 2.5. `regenerations`
Tracks every time a user hits the AI "Regenerate" button. Crucial for enforcing the "Max 3 per section" rule and calculating final costs.

| Column Name | Data Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | UUID | PRIMARY KEY | - |
| `resume_id` | UUID | FK -> `resumes(id)` | Which resume this belongs to. |
| `section_identifier` | VARCHAR(100) | NOT NULL | e.g., `WORK_EXP_ID_1`, `SUMMARY`. |
| `user_prompt` | TEXT | NULL | e.g., "Make it sound more professional". |
| `cost_egp` | DECIMAL(10,2) | NOT NULL | Added cost (e.g., 10 EGP). |
| `cost_usd` | DECIMAL(10,2) | NOT NULL | Added cost (e.g., 0.25 USD). |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | - |

*Backend Logic Note: Before inserting here, the .NET backend will query: `SELECT COUNT(*) FROM regenerations WHERE resume_id = @id AND section_identifier = @section_id`. If >= 3, reject request.*

#### 2.6. `transactions`
Handles Stripe/Paymob payments before allowing a document download.

| Column Name | Data Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | UUID | PRIMARY KEY | Internal transaction ID. |
| `user_id` | UUID | FK -> `users(id)` | Who paid. |
| `resume_id` | UUID | FK -> `resumes(id)` | What they paid for. |
| `base_amount` | DECIMAL(10,2) | NOT NULL | Template base price at time of checkout. |
| `regen_amount` | DECIMAL(10,2) | NOT NULL | Total cost accumulated from AI usage. |
| `total_amount` | DECIMAL(10,2) | NOT NULL | base_amount + regen_amount. |
| `currency` | VARCHAR(3) | NOT NULL | `EGP` or `USD`. |
| `payment_status`| VARCHAR(20) | NOT NULL | `PENDING`, `SUCCESS`, `FAILED`. |
| `gateway_ref_id`| VARCHAR(255) | NULL | Stripe Session ID / Paymob Order ID. |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | - |
| `completed_at` | TIMESTAMPTZ | NULL | Timestamp of webhook fulfillment. |

#### 2.7. `downloads` (Suggested Additional Table)
Keeps track of when a user actually downloads their paid resume, which format, and prevents infinite server abuse.

| Column Name | Data Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | UUID | PRIMARY KEY | - |
| `resume_id` | UUID | FK -> `resumes(id)` | Document downloaded. |
| `format_type` | VARCHAR(10) | NOT NULL | `PDF` or `DOCX`. |
| `downloaded_at`| TIMESTAMPTZ | DEFAULT NOW() | Time of download request. |
| `ip_address` | VARCHAR(45) | NULL | For abuse prevention. |


*Note: Generating a unique `id` for each work/education item inside the JSON is important so you can link it to the `section_identifier` in the `regenerations` table.*
