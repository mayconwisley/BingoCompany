import { useEffect, useState } from "react";
import QRCode from "qrcode";
import { useParams } from "react-router-dom";
import { bingoApi } from "../features/bingo/bingoApi";
import { useAsyncResource } from "../shared/hooks/useAsyncResource";
import { AppShell } from "../shared/ui/AppShell";
import { PageState } from "../shared/ui/PageState";
import "./printCardsPage.css";

type PrintableCard = { publicCode: string; fingerprint: string; companyName: string; numbers: number[][]; qrCode: string };

export function PrintCardsPage()
{
    const { eventId = "" } = useParams();
    const cards = useAsyncResource(() => bingoApi.getPrintedCards(eventId), [eventId]);
    const [printableCards, setPrintableCards] = useState<PrintableCard[]>([]);

    useEffect(() =>
    {
        if (!cards.data) return;
        void Promise.all(cards.data.map(async card => ({ ...card, qrCode: await QRCode.toDataURL(card.qrCodeValue, { margin: 1, width: 180 }) }))).then(setPrintableCards);
    }, [cards.data]);

    return <AppShell><main className="printcards"><header className="pagehead"><div><p className="eyebrow">Cartelas impressas</p><h1>Prontas para impressão</h1></div><button className="primary" onClick={() => window.print()}>Imprimir / salvar PDF</button></header><PageState loading={cards.loading} error={cards.error} /><div className="printgrid">{printableCards.map(card => <article className="printcard" key={card.publicCode}><header><strong>BINGO</strong><span>{card.publicCode}</span></header><div className="printnumbers">{card.numbers.flatMap((row, rowIndex) => row.map((number, columnIndex) => <span key={`${rowIndex}-${columnIndex}`} className={number === 0 ? "free" : ""}>{number === 0 ? "★" : number}</span>))}</div><footer className="printcard-footer"><img src={card.qrCode} alt={`QR Code da cartela ${card.publicCode}`} /><div className="printcard-details"><p className="printcard-company">{card.companyName}</p><small>Fingerprint: {card.fingerprint}</small></div></footer></article>)}</div></main></AppShell>;
}
