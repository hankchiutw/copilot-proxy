export async function AuthMiddleware(
    request: Request
) {
    const apiKey =
        request.headers.get(
            "x-monstertools-key"
        );

    if(
        process.env.MONSTERTOOLS_KEY &&
        apiKey !== process.env.MONSTERTOOLS_KEY
    ) {
        throw new Response(
            "Unauthorized",
            { status:401 }
        );
    }
}