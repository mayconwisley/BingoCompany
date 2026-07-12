import { HubConnectionBuilder } from "@microsoft/signalr";
import { useEffect, useState } from "react";
import { API_URL } from "../../shared/api/httpClient";

export function useLiveBingo(eventId?: string, roundId?: string, onChange?: () => void)
{
    const [state, setState] = useState("Conectando");

    useEffect(() =>
    {
        if (!eventId) return;

        const connection = new HubConnectionBuilder().withUrl(`${API_URL}/hubs/bingo`).withAutomaticReconnect().build();
        ["NumberDrawn", "WinningCardDetected", "WinnerRevealed", "WinnerPresentationClosed", "RoundStarted"].forEach(event => connection.on(event, () => onChange?.()));
        connection.onreconnecting(() => setState("Reconectando"));
        connection.onreconnected(() => { setState("Conectado"); onChange?.(); });
        connection.onclose(() => setState("Desconectado"));
        void connection.start().then(async () =>
        {
            await connection.invoke("JoinEvent", eventId);
            if (roundId) await connection.invoke("JoinRound", roundId);
            setState("Conectado");
        }).catch(() => setState("Sem conexão"));

        return () => { void connection.stop(); };
    }, [eventId, roundId, onChange]);

    return state;
}
