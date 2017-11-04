// No real better place to put these... We should generate declarations for all API models.
export interface IPaginationMetadata {
    count: number;

    previous: string;

    next: string;
}

export interface IUpload {
    id: number;

    timeSubmitted: string;

    playStyle: string;

    user?: IUser;

    uploadContent?: string;

    stats?: { [key: string]: string };
}

export interface IUser {
    id: string;

    name: string;
}
