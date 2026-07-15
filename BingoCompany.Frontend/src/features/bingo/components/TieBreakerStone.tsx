import { useEffect, useState } from "react";
import { bingoBallLabel } from "../model/bingoBallLabel";

const suspenseNumbers = [7, 18, 29, 41, 52, 63, 74];
const revealDelayInMilliseconds = 900;
const suspenseFrameInMilliseconds = 120;

type TieBreakerStoneProps = {
	number: number;
	delay: number;
};

export function TieBreakerStone({ number, delay }: TieBreakerStoneProps) {
	const [displayedNumber, setDisplayedNumber] = useState(suspenseNumbers[number % suspenseNumbers.length]);
	const [isRevealed, setIsRevealed] = useState(false);
	const [isRolling, setIsRolling] = useState(false);

	useEffect(() => {
		setIsRevealed(false);
		setIsRolling(false);
		let frame = number % suspenseNumbers.length;
		let interval: number | undefined;
		let revealTimeout: number | undefined;
		const startTimeout = window.setTimeout(() => {
			setIsRolling(true);
			interval = window.setInterval(() => {
				frame = (frame + 1) % suspenseNumbers.length;
				setDisplayedNumber(suspenseNumbers[frame]);
			}, suspenseFrameInMilliseconds);
			revealTimeout = window.setTimeout(() => {
				if (interval) window.clearInterval(interval);
				setDisplayedNumber(number);
				setIsRolling(false);
				setIsRevealed(true);
			}, revealDelayInMilliseconds);
		}, delay);

		return () => {
			window.clearTimeout(startTimeout);
			if (revealTimeout) window.clearTimeout(revealTimeout);
			if (interval) window.clearInterval(interval);
		};
	}, [delay, number]);

	return (
		<strong className={isRolling ? "tie-breaker-stone" : "tie-breaker-stone revealed"}>
			{isRevealed ? bingoBallLabel(number) : bingoBallLabel(displayedNumber)}
		</strong>
	);
}
