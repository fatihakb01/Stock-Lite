import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { loginSchema, type LoginFormValues } from "./schemas/loginSchema";
import { useLogin } from "../../hooks/useUsers";
import { Link, useNavigate } from "react-router-dom";
import { useState } from "react";
import { LockOpen } from "@mui/icons-material";
import TextInput from "../../components/forms/TextInput"; 
import {
  Box,
  Button,
  Paper,
  Typography,
  CircularProgress,
} from "@mui/material";

export default function LoginPage() {
  const navigate = useNavigate();
  const { mutateAsync: loginUser } = useLogin();

  const [notVerified, setNotVerified] = useState(false);

  const {
    control,
    handleSubmit,
    formState: { isValid, isSubmitting }
  } = useForm<LoginFormValues>({
    mode: "onTouched",
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: ""
    }
  });

  const onSubmit = async (data: LoginFormValues) => {
    try {
      const res = await loginUser(data);

      // Save token & user
      localStorage.setItem("token", res.token);
      localStorage.setItem("user", JSON.stringify(res.user));

      navigate("/home");
    } catch (err: any) {
      if (err.message === "NotAllowed") {
        setNotVerified(true);
      }
    }
  };

  return (
    <Box
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="100vh"
      minWidth="100vw"
      p={2}
    >
      <Paper
        component="form"
        onSubmit={handleSubmit(onSubmit)}
        sx={{
          display: "flex",
          flexDirection: "column",
          p: 4,
          gap: 3,
          maxWidth: 450,
          width: "100%",
          borderRadius: 3,
        }}
      >
        {/* Header */}
        <Box
          display="flex"
          alignItems="center"
          justifyContent="center"
          gap={2}
          color="primary.main"
        >
          <LockOpen fontSize="large" />
          <Typography variant="h4">Login</Typography>
        </Box>

        {/* Email */}
        <TextInput
          name="email"
          label="Email"
          control={control}
        />

        {/* Password */}
        <TextInput
          name="password"
          label="Password"
          type="password"
          control={control}
        />

        {/* Submit Button */}
        <Button
          type="submit"
          disabled={!isValid || isSubmitting}
          variant="contained"
          size="large"
          fullWidth
          startIcon={isSubmitting ? <CircularProgress size={20} /> : null}
        >
          {isSubmitting ? "Logging in..." : "Login"}
        </Button>

        {/* If email not verified */}
        {notVerified ? (
          <Box textAlign="center">
            <Typography color="error" mb={1}>
              Your email is not verified.  
              Please confirm your email.
            </Typography>

            {/* Add resend confirmation later */}
          </Box>
        ) : (
          <>
            {/* Forgot password */}
            <Typography textAlign="center">
              Forgot password?{" "}
              <Link to="/forgot-password">Click here</Link>
            </Typography>

            {/* Register link */}
            {/* <Typography textAlign="center">
              Don't have an account?{" "}
              <Link to="/register">Sign up</Link>
            </Typography> */}
          </>
        )}
      </Paper>
    </Box>
  );
}
