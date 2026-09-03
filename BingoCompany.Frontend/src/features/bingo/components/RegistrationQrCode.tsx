import { useEffect, useState } from "react";
import QRCode from "qrcode";

type Props = {
	registrationPath: string;
	description?: string;
};

export function RegistrationQrCode({ registrationPath, description = "Aponte a câmera para se inscrever e gerar sua cartela." }: Props) {
	const [imageUrl, setImageUrl] = useState("");

	useEffect(() => {
		const applicationBasePath = import.meta.env.BASE_URL.replace(/\/$/, "");
		const registrationUrl = new URL(`${applicationBasePath}${registrationPath}`, window.location.origin).toString();
		void QRCode.toDataURL(registrationUrl, { margin: 1, width: 180 }).then(setImageUrl);
	}, [registrationPath]);

	return imageUrl ? (
		<figure className="registration-qr">
			<img src={imageUrl} alt="QR Code para inscrição no bingo" />
			<figcaption>{description}</figcaption>
		</figure>
	) : null;
}
