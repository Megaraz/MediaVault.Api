
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


export default class UsersClient {

    private baseUrl = "/users";



    async getAllUsers(): Promise<UserDetailedDto[]> {
        const response = await fetch(this.baseUrl);
        if (!response.ok) {
            throw new Error("Failed to fetch users");
        }
        return response.json();
    }

    async login(credentials: UserLoginDto): Promise<UserDetailedDto> {
        const response = await fetch(this.baseUrl + "/login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(credentials)
        });
        if (!response.ok) {
            throw new Error("Failed to login");
        }
        return response.json();
    }


    async registerUser(user: UserCreateDto): Promise<UserDetailedDto> {
        const response = await fetch(this.baseUrl + "/register", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(user)
        });
        if (!response.ok) {
            throw new Error("Failed to register user");
        }
        return response.json();
    }
}
