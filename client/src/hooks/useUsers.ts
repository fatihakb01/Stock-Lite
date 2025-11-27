import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  login,
  registerUser,
  getUsers,
  getUser,
  getCurrentUser,
  resetPassword,
  changePassword,
  confirmEmail,
  editUser,
  deleteUser
} from "../api/usersApi";

import type {
  LoginDto,
  RegisterUserDto,
  ForgotPasswordDto,
  ChangePasswordDto,
  ConfirmEmailDto,
  User,
  AuthResponse,
  EditUserDto,
  ConfirmEmailResult
} from "../features/users/userTypes";

// LOGIN
export const useLogin = () => {
  return useMutation<AuthResponse, Error, LoginDto>({
    mutationFn: login,
  });
};

// REGISTER USER
export const useRegisterUser = () => {
  const queryClient = useQueryClient();

  return useMutation<User, Error, RegisterUserDto>({
    mutationFn: registerUser,
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["users"] }),
  });
};

// EDIT USER
export const useEditUser = (id: string | null) => {
  const queryClient = useQueryClient();

  return useMutation<User, Error, EditUserDto>({
    mutationFn: (data) => editUser(id!, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      queryClient.invalidateQueries({ queryKey: ["user", id] });
    },
  });
};

// DELETE USER
export const useDeleteUser = (id: string | null) => {
  const queryClient = useQueryClient();

  return useMutation<void>({
    mutationFn: () => deleteUser(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
    }
  });
};

// CURRENT USER
export const useCurrentUser = () => {
  return useQuery<User>({
    queryKey: ["currentUser"],
    queryFn: getCurrentUser,
  });
};

// GET ALL USERS
export const useUsers = (companyId: string | null) => {
  return useQuery<User[]>({
    queryKey: ["users", companyId],
    queryFn: () => getUsers(companyId!),
    enabled: !!companyId
  });
};

// GET USER BY ID
export const useUser = (id: string | null) => {
  return useQuery<User>({
    queryKey: ["user", id],
    queryFn: () => getUser(id!),
    enabled: !!id
  });
};

// RESET PASSWORD
export const useResetPassword = () => {
  return useMutation<void, Error, ForgotPasswordDto>({
    mutationFn: resetPassword
  });
};

// CHANGE PASSWORD
export const useChangePassword = () => {
  return useMutation<void, Error, ChangePasswordDto>({
    mutationFn: changePassword
  });
};

// CONFIRM EMAIL
export const useConfirmEmail = () => {
  return useMutation<ConfirmEmailResult, Error, ConfirmEmailDto>({
    mutationFn: confirmEmail
  });
};
