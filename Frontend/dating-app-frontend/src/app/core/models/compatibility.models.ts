import { Hobby } from './hobby.models';

export interface CompatibleUser {
  id: string;
  firstName: string;
  mbtiCode: number;
  hobbies: Hobby[];
}

export interface Compatibility {
  id: number;
  userId_A: string;
  userId_B: string;
  userA: CompatibleUser | null;
  userB: CompatibleUser | null;
}
