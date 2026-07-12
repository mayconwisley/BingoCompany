export class ApiError extends Error { constructor(message:string,public readonly status:number){super(message)} }
export const API_URL=import.meta.env.VITE_API_URL??"http://localhost:5138";
export async function http<T>(path:string,init?:RequestInit):Promise<T>{const response=await fetch(`${API_URL}${path}`,{...init,headers:{"Content-Type":"application/json",...init?.headers}});if(!response.ok)throw new ApiError((await response.text())||"Não foi possível concluir a operação.",response.status);return response.status===204?undefined as T:response.json()}
