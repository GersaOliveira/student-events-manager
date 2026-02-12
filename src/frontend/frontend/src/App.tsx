import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Login } from './pages/Login';
import { Dashboard } from './pages/Dashboard';
import './index.css'; // Onde o Tailwind está configurado

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Rota inicial: Tela de Login */}
        <Route path="/login" element={<Login />} />
        
        {/* Rota do sistema: Dashboard com a lista de alunos */}
        <Route path="/dashboard" element={<Dashboard />} />
        
        {/* Redirecionamento padrão caso a rota não exista */}
        <Route path="*" element={<Navigate to="/login" />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;