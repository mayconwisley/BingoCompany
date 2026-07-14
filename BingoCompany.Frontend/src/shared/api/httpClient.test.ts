import { beforeEach, describe, expect, it, vi } from "vitest";

const { clearSession } = vi.hoisted(() => ({ clearSession: vi.fn() }));

vi.mock("../auth/session", () => ({ clearSession }));

import { API_URL, http } from "./httpClient";

describe("http", () => {
    beforeEach(() => {
        clearSession.mockReset();
        vi.stubGlobal("fetch", vi.fn());
    });

    it("inclui o cookie de autenticação nas chamadas para a API", async () => {
        vi.mocked(fetch).mockResolvedValue(new Response(JSON.stringify({ id: "event-1" }), { status: 200 }));

        await http<{ id: string }>("/api/events");

        expect(fetch).toHaveBeenCalledWith(
            `${API_URL}/api/events`,
            expect.objectContaining({ credentials: "include" })
        );
    });
});
