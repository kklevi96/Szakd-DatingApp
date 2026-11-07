export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  birthDay: string;
  wantsRelationship: boolean;
  wantsFriendship: boolean;
  interestedinWoman: boolean;
  interestedinMan: boolean;
  gender: string;
  mbtiCode: number;
}

export interface RegisterResponse {
  succeeded: boolean;
  errors: {
    code: string;
    description: string;
  }[];
}

export interface LoginResponse {
  token: string;
  expiration: string;
  id: string;
  email: string;
}

export interface AdminResponse {
  message: string;
}

export interface SearchPreferences {
  wantsRelationship: boolean;
  wantsFriendship: boolean;
}
