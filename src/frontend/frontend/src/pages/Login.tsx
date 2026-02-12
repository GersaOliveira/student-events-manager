import { useState } from 'react';
import { authService } from '../services/authService';

export const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      // Conecta com o backend
      const data = await authService.login(email, password);
      
      // Salva o Token JWT no navegador
      localStorage.setItem('token', data.token.token);
      
      // Redireciona para o Dashboard
      window.location.href = '/dashboard';
    } catch (error) {
      alert('Falha no login! Verifique se a API está ativa e se as credenciais estão corretas.');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100">
      <form onSubmit={handleLogin} className="bg-white p-10 rounded-xl shadow-lg w-96 text-center">
        <h2 className="text-2xl font-bold mb-6 text-blue-700">Login Acadêmico</h2>
        <input 
          type="email" 
          placeholder="E-mail" 
          className="w-full p-3 border rounded mb-4 outline-none focus:ring-2 focus:ring-blue-500"
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        <input 
          type="password" 
          placeholder="Senha" 
          className="w-full p-3 border rounded mb-6 outline-none focus:ring-2 focus:ring-blue-500"
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        <button type="submit" className="w-full bg-blue-600 text-white p-3 rounded-lg font-bold hover:bg-blue-700 transition">
          Entrar no Sistema
        </button>
      </form>
    </div>
  );
};