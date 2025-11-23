export type User = {
    id: string
    firstName?: string
    lastName?: string
    userName: string
    email: string
    password: string
    phoneNumber?: string
    emailConfirmed: boolean
    role: string
    createdAt: string
    isActive: boolean
    companyId: string
}

export type LoginDto = {
  email: string
  password: string
}

export type AuthResponse = {
  user: {
    id: string
    firstName: string
    lastName: string
    userName: string
    email: string
    role: string
    companyId: string
  }
  token: string
}

export type RegisterUserDto = {
  firstName?: string
  lastName?: string
  userName: string
  email: string
  password: string
  phoneNumber?: string
  role: string
  companyId: string
}

export type ForgotPasswordDto = {
  email: string
}

export type ChangePasswordDto = {
  userId: string
  token: string
  newPassword: string
}

export type ConfirmEmailDto = {
  userId: string
  token: string
}
