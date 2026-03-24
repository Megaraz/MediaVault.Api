import { useState } from "react";
import type { UserCreateDto } from "../../Clients/UsersClient";
import ModularPopupWindow from "../Shared/ModularPopupWindow";
import RegisterUserForm, { RegisterUserFormData } from "./RegisterUserForm";

type RegisterProps = {
  onSubmit: (data: UserCreateDto) => void;
  onCancel: () => void;
};

export default function RegisterUser({ onSubmit, onCancel }: RegisterProps) {
  const [formData, setFormData] = useState<RegisterUserFormData>(
    new RegisterUserFormData(),
  );

  const handleSubmit = (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (formData.email !== formData.confirmEmail) {
      alert("Emails do not match");
      return;
    }

    if (formData.password !== formData.confirmPassword) {
      alert("Passwords do not match");
      return;
    }

    const dto: UserCreateDto = {
      username: formData.username,
      email: formData.email,
      confirmEmail: formData.confirmEmail,
      password: formData.password,
      confirmPassword: formData.confirmPassword,
    };

    onSubmit(dto);
  };

  const handleChange = (field: keyof RegisterUserFormData, value: string) => {
    setFormData((prev) => {
      return { ...prev, [field]: value };
    });
  };

  return (
    <ModularPopupWindow onClose={onCancel}>
      <form className="p-6 space-y-6" onSubmit={handleSubmit}>
        <RegisterUserForm formData={formData} onChange={handleChange} />
      </form>
    </ModularPopupWindow>
  );
}
