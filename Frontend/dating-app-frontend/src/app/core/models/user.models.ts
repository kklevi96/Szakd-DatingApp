import { Hobby } from './hobby.models';

export class User {
  id = '';
  firstName = '';
  age = 0;
  wantsRelationship = false;
  wantsFriendship = false;
  interestedinWoman = false;
  interestedinMan = false;
  gender = '';
  mbtiCode = 0;
  hobbies: Hobby[] = [];
}
