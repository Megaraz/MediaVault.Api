
export type UserDetailedDto = {
    id: string;
    username: string;
    email: string;
    createdAt: string;
}

export type UserCreateDto = {
    username: string;
    email: string;
    confirmEmail: string;
    password: string;
    confirmPassword: string;
}

export type UserLoginDto = {
    email: string;
    password: string;
}

type ApiErrorResponse = {
    message?: string;
    errorCode?: string;
    errors?: string[];
}

export default class UsersClient {
    private authBaseUrl = "/auth";
    private usersBaseUrl = "/users";

    private async readResponse<T>(response: Response): Promise<T> {
        if (!response.ok) {
            let errorMessage = "Request failed";

            try {
                const error = (await response.json()) as ApiErrorResponse;
                errorMessage = error.message ?? error.errors?.join(", ") ?? errorMessage;
            } catch {
                errorMessage = response.statusText || errorMessage;
            }

            throw new Error(errorMessage);
        }

        if (response.status === 204) {
            return undefined as T;
        }

        return response.json() as Promise<T>;
    }

    async getUsers(pageNumber: number = 1, pageSize: number = 10): Promise<UserDetailedDto[]> {
        const response = await fetch(
            `${this.usersBaseUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`,
            {
                credentials: "include"
            }
        );

        return this.readResponse<UserDetailedDto[]>(response);
    }

    async login(credentials: UserLoginDto): Promise<UserDetailedDto> {
        const response = await fetch(this.authBaseUrl + "/login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            credentials: "include",
            body: JSON.stringify(credentials)
        });

        return this.readResponse<UserDetailedDto>(response);
    }

    async registerUser(user: UserCreateDto): Promise<UserDetailedDto> {
        const response = await fetch(this.authBaseUrl + "/register", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            credentials: "include",
            body: JSON.stringify(user)
        });

        return this.readResponse<UserDetailedDto>(response);
    }

    async logout(): Promise<void> {
        const response = await fetch(this.authBaseUrl + "/logout", {
            method: "POST",
            credentials: "include"
        });

        await this.readResponse<void>(response);
    }

    async getCurrentUser(): Promise<UserDetailedDto> {
        const response = await fetch(this.authBaseUrl + "/me", {
            credentials: "include"
        });

        return this.readResponse<UserDetailedDto>(response);
    }
}
