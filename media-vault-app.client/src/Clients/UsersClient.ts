
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


export default class UsersClient {

    private baseUrl = "/users";

    async getAllUsers(): Promise<UserDetailedDto[]> {
        const response = await fetch(this.baseUrl);
        if (!response.ok) {
            throw new Error("Failed to fetch users");
        }
        return response.json();
    }

    async createUser(user: UserCreateDto): Promise<UserDetailedDto> {
        const response = await fetch(this.baseUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(user)
        });
        if (!response.ok) {
            throw new Error("Failed to create user");
        }
        return response.json();
    }
}
