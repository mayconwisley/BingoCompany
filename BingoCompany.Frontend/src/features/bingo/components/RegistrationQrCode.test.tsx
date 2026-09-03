import { render, screen } from "@testing-library/react";
import QRCode from "qrcode";
import { afterEach, describe, expect, it, vi } from "vitest";
import { RegistrationQrCode } from "./RegistrationQrCode";

vi.mock("qrcode", () => ({
	default: {
		toDataURL: vi.fn().mockResolvedValue("data:image/png;base64,qr-code")
	}
}));

describe("RegistrationQrCode", () => {
	afterEach(() => {
		vi.unstubAllEnvs();
		vi.clearAllMocks();
	});

	it("inclui o caminho base publicado na URL codificada", async () => {
		vi.stubEnv("BASE_URL", "/bingo/");

		render(<RegistrationQrCode registrationPath="/participar/ABC123" />);

		await screen.findByRole("img", { name: "QR Code para inscrição no bingo" });
		expect(QRCode.toDataURL).toHaveBeenCalledWith("http://localhost:3000/bingo/participar/ABC123", { margin: 1, width: 180 });
	});

	it("exibe a orientação específica quando informada", async () => {
		render(<RegistrationQrCode registrationPath="/participar/ABC123" description="Abra a inscrição pública pelo convite." />);

		expect(await screen.findByText("Abra a inscrição pública pelo convite.")).toBeInTheDocument();
	});
});
