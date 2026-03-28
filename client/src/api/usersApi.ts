import api from "./axios";
import type {
  LoginDto,
  AuthResponse,
  RegisterUserDto,
  User,
  ForgotPasswordDto,
  ChangePasswordDto,
  ConfirmEmailDto,
  EditUserDto,
  ConfirmEmailResult,
  CurrentUser
} from "../types/users/userTypes";

// LOGIN
export const login = async (data: LoginDto): Promise<AuthResponse> => {
  const res = await api.post("/user/login", data);
  return res.data;
};

// REGISTER
export const registerUser = async (data: RegisterUserDto): Promise<User> => {
  const res = await api.post("/user/register", data);
  return res.data;
};

// EDIT
export const editUser = async (id: string, data: EditUserDto): Promise<User> => {
  const res = await api.put(`/user/edit/${id}`, data);
  return res.data;
};

// DELETE
export const deleteUser = async (id: string): Promise<void> => {
  await api.delete(`/user/delete/${id}`);
};

// CONFIRM EMAIL
export const confirmEmail = async (data: ConfirmEmailDto): Promise<ConfirmEmailResult> => {
  const res = await api.post("/user/confirm-email", data);
  return res.data;
};

// FORGOT PASSWORD
export const resetPassword = async (data: ForgotPasswordDto): Promise<void> => {
  await api.post("/user/reset-password", data);
};

// CHANGE PASSWORD
export const changePassword = async (data: ChangePasswordDto): Promise<void> => {
  await api.post("/user/change-password", data);
};

// GET CURRENT USER
export const getCurrentUser = async (): Promise<CurrentUser> => {
  const res = await api.get("/user/current");
  return res.data;
};

// GET ALL USERS FOR A COMPANY
export const getUsers = async (): Promise<User[]> => {
  const res = await api.get('/user/all');
  return res.data;
};

// GET USER BY ID
export const getUser = async (id: string): Promise<User> => {
  const res = await api.get(`/user/${id}`);
  return res.data;
};
