import api from "./axios";
import type {
  LoginDto,
  AuthResponse,
  RegisterUserDto,
  User,
  ForgotPasswordDto,
  ChangePasswordDto,
  ConfirmEmailDto
} from "../features/users/userTypes";

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

// CONFIRM EMAIL
export const confirmEmail = async (data: ConfirmEmailDto): Promise<User> => {
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
export const getCurrentUser = async (): Promise<User> => {
  const res = await api.get("/user/current");
  return res.data;
};

// GET ALL USERS FOR A COMPANY
export const getUsers = async (companyId: string): Promise<User[]> => {
  const res = await api.get(`/user/all?companyId=${companyId}`);
  return res.data;
};

// GET USER BY ID
export const getUser = async (id: string): Promise<User> => {
  const res = await api.get(`/user/${id}`);
  return res.data;
};
