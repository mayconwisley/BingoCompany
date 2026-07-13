import { http } from "../../shared/api/httpClient";

export type ApplicationInfo = {
    name: string;
    description: string;
    version: string;
};

export const applicationInfoApi = {
    get: () => http<ApplicationInfo>("/api/application/info")
};
