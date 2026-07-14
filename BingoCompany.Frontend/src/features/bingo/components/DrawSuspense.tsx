import { useEffect, useRef, useState } from "react";
import "./drawSuspense.css";

const suspenseNumbers = [7, 18, 29, 41, 52, 63, 74];
const suspenseDurationInMilliseconds = 1600;
const suspenseFrameInMilliseconds = 120;

type DrawSuspenseProps = {
	number?: number;
};

export function DrawSuspense({ number }: DrawSuspenseProps) {
	const [displayedNumber, setDisplayedNumber] = useState<number | undefined>(number);
	const [isSuspending, setIsSuspending] = useState(false);
	const previousNumber = useRef<number | undefined>(number);
	const isFirstNumber = useRef(true);

	useEffect(() => {
		if (number === undefined) {
			previousNumber.current = undefined;
			isFirstNumber.current = true;
			setDisplayedNumber(undefined);
			setIsSuspending(false);
			return;
		}

		if (isFirstNumber.current) {
			isFirstNumber.current = false;
			previousNumber.current = number;
			setDisplayedNumber(number);
			return;
		}

		if (previousNumber.current === number) return;

		previousNumber.current = number;
		let frame = number % suspenseNumbers.length;
		setDisplayedNumber(suspenseNumbers[frame]);
		setIsSuspending(true);
		const interval = window.setInterval(() => {
			frame = (frame + 1) % suspenseNumbers.length;
			setDisplayedNumber(suspenseNumbers[frame]);
		}, suspenseFrameInMilliseconds);
		const timeout = window.setTimeout(() => {
			window.clearInterval(interval);
			setDisplayedNumber(number);
			setIsSuspending(false);
		}, suspenseDurationInMilliseconds);

		return () => {
			window.clearInterval(interval);
			window.clearTimeout(timeout);
		};
	}, [number]);

	return (
		<section className={isSuspending ? "drawsuspense rolling" : "drawsuspense"} aria-live="polite">
			<p>{isSuspending ? "A próxima pedra é..." : "ÚLTIMA PEDRA"}</p>
			<strong>{displayedNumber ?? "?"}</strong>
			{isSuspending && <span>Preparando o sorteio</span>}
		</section>
	);
}
