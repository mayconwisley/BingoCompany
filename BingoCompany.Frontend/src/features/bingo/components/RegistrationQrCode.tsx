import { useEffect, useState } from "react";
import QRCode from "qrcode";

type Props = { registrationPath: string };

export function RegistrationQrCode({ registrationPath }: Props) {
    const [imageUrl, setImageUrl] = useState("");

    useEffect(() => {
        const registrationUrl = new URL(registrationPath, window.location.origin).toString();
        void QRCode.toDataURL(registrationUrl, { margin: 1, width: 180 }).then(setImageUrl);
    }, [registrationPath]);

    return imageUrl ? (
        <figure className="registration-qr">
            <img src={imageUrl} alt="QR Code para inscrição no bingo" />
            <figcaption>Aponte a câmera para se inscrever e gerar sua cartela.</figcaption>
        </figure>
    ) : null;
}
