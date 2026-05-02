# Diagram: Payment & Checkout Sequence
**Location:** `Documentations/resources/flow-diagrams/05-Payment-Sequence.md`  
**Scope:** Full payment checkout flow from "Proceed to Pay" through webhook fulfillment and download unlock (SRD §3.2.5, §3.3).

---

## Sequence Diagram

![Checkout & Webhook Sequence](../../images/Checkout%20&%20Webhook%20Sequence.png)

---

## Download Flow (Post-Payment)

![Secure Download Flow](../../images/Secure%20Download%20Flow.png)

---

## Pricing Calculation Reference

| Line Item | Source | Example (EGP) |
| :--- | :--- | :--- |
| Template base price | `templates.base_price_egp` at checkout time | 120.00 |
| AI regeneration charges | `SUM(regenerations.cost_egp)` for this resume | 30.00 |
| **Total charged** | base + regen | **150.00** |

> Currency is determined by user IP at session start (Req 3.2.5.4). EGP via Paymob, USD via Stripe.
