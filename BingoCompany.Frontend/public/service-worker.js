const CACHE_NAME = "bingo-company-shell-v1";
const API_PATH_SEGMENT = "/api/";
const HUB_PATH_SEGMENT = "/hubs/";

self.addEventListener("install", () => {
	self.skipWaiting();
});

self.addEventListener("activate", (event) => {
	event.waitUntil(self.clients.claim());
});

self.addEventListener("fetch", (event) => {
	const request = event.request;
	const url = new URL(request.url);
	if (request.method !== "GET" || url.origin !== self.location.origin || url.pathname.includes(API_PATH_SEGMENT) || url.pathname.includes(HUB_PATH_SEGMENT)) {
		return;
	}

	if (request.mode === "navigate") {
		event.respondWith(networkFirstNavigation(request));
		return;
	}

	event.respondWith(staleWhileRevalidate(request));
});

async function networkFirstNavigation(request) {
	const cache = await caches.open(CACHE_NAME);
	try {
		const response = await fetch(request);
		if (response.ok) {
			await cache.put(request, response.clone());
			await cache.put(new URL("index.html", self.registration.scope), response.clone());
		}
		return response;
	} catch {
		return (await cache.match(request)) ?? (await cache.match(new URL("index.html", self.registration.scope))) ?? Response.error();
	}
}

async function staleWhileRevalidate(request) {
	const cache = await caches.open(CACHE_NAME);
	const cached = await cache.match(request);
	const networkRequest = fetch(request)
		.then(async (response) => {
			if (response.ok) await cache.put(request, response.clone());
			return response;
		})
		.catch(() => undefined);

	return cached ?? (await networkRequest) ?? Response.error();
}
