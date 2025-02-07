export interface User {
    userId : number,
    nickname : string,
    email : string,
    image : string,
    role: string,
    is_banned: boolean,
    avatarUrl?: string
}
