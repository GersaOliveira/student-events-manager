import api from './api';

export interface Event {
  id: string;
  subject: string;
  startTime: string;
  endTime: string;
}

export interface Student {
  id: string;
  name: string;
  email: string;
  events: Event[];
}

export const studentService = {
  getAll: async () => {
    const response = await api.get<Student[]>('/Students/external');
    return response.data;
  }
};