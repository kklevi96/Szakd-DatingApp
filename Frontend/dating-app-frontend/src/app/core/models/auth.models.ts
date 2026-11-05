export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  birthDay: string;
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
