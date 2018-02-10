// No real better place to put these... We should generate declarations for all API models.
export interface IPaginationMetadata {
    count: number;

    previous: string;

    next: string;
}

export interface IUser {
    name: string;

    clanName: string;
}
