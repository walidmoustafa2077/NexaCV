import { apiFetch } from "./client";
import type { CheckoutRequest, CheckoutResponse, TransactionDto } from "@/types/api.types";

export function checkout(data: CheckoutRequest): Promise<CheckoutResponse> {
    return apiFetch<CheckoutResponse>("/api/transactions/checkout", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

export function getTransaction(id: string): Promise<TransactionDto> {
    return apiFetch<TransactionDto>(`/api/transactions/${id}`);
}
