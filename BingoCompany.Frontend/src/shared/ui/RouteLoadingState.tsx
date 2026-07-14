import "./routeLoadingState.css";

export function RouteLoadingState() {
	return (
		<main className="route-loading-state" aria-busy="true">
			<section className="route-loading-state__content" role="status" aria-live="polite">
				<span className="route-loading-state__mark" aria-hidden="true">
					B
				</span>
				<div>
					<p className="route-loading-state__eyebrow">Bingo Company</p>
					<p className="route-loading-state__message">Preparando sua experiência de bingo...</p>
				</div>
			</section>
		</main>
	);
}
