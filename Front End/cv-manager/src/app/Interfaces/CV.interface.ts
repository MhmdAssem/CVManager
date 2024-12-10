import { ExperienceInformation } from "./ExperienceInformation.interface";
import { PersonalInformation } from "./PersonalInformation.interface";

export interface CV {
    id?: number;
    name: string;
    personalInformation: PersonalInformation;
    experienceInformation: ExperienceInformation[];
  }