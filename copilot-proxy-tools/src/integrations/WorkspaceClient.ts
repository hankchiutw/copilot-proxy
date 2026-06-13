export class WorkspaceClient {
    static resolve(
        request: Request
    ): string {

        return (
            request.headers.get(
                "x-workspace-root"
            )
            ?? process.cwd()
        );
    }
}