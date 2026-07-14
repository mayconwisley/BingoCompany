export function ConnectionBadge({ status }: { status: string }) {
	return (
		<span className="online" aria-label={`Conexão: ${status}`}>
			● {status}
		</span>
	);
}
