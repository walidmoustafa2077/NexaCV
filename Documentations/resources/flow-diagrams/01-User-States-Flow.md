# Diagram: User States Flow
**Location:** `Documentations/resources/flow-diagrams/01-User-States-Flow.md`  
**Scope:** All possible application modes a user can be in, matching SRD §3.1.

---

![User States Flow](../../images/User%20States%20Flow.png)

---

## State Transition Table

| From State | To State | Trigger | Actor |
| :--- | :--- | :--- | :--- |
| Guest | Authenticated | Register / Login form submitted | User |
| Guest | Authenticated | Template selected (auth wall) | System |
| Authenticated | Processing | Wizard "Generate Resume" clicked | User |
| Processing | Review/Edit | OpenAI API returns success | System |
| Processing | Review/Edit | OpenAI times out (>5s) — manual fallback | System |
| Review/Edit | Processing | "Regenerate" clicked, count < 3 | User |
| Review/Edit | Checkout | "Proceed to Payment" clicked | User |
| Checkout | Fulfilled | Stripe/Paymob webhook fires `SUCCESS` | System |
| Checkout | Review/Edit | Payment `FAILED` or user cancels | System / User |
| Fulfilled | Review/Edit | User edits previously-paid resume | User |
| Authenticated | Guest | Logout | User |
