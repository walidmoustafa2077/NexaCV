import { useQuery } from "@tanstack/react-query";
import { getMe } from "@/lib/api/users";
import { queryKeys } from "@/lib/query/keys";

export function useUser() {
    return useQuery({
        queryKey: queryKeys.user(),
        queryFn: getMe,
    });
}
