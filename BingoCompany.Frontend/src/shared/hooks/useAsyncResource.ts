import { useCallback, useEffect, useState } from "react";
import { getErrorMessage } from "../api/getErrorMessage";

export function useAsyncResource<T>(loader: () => Promise<T>, dependencies: unknown[])
{
    const [data, setData] = useState<T>();
    const [error, setError] = useState<string>();
    const [loading, setLoading] = useState(true);
    const reload = useCallback(async () =>
    {
        setLoading(true);
        try
        {
            setData(await loader());
            setError(undefined);
        }
        catch (error)
        {
            setError(getErrorMessage(error, "Não foi possível carregar estas informações. Tente novamente."));
        }
        finally
        {
            setLoading(false);
        }
    }, dependencies);

    useEffect(() => { void reload(); }, [reload]);
    return { data, error, loading, reload, setData };
}
