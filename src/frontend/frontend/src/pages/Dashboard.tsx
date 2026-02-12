import { useEffect, useState } from 'react';
import { studentService, type Student } from '../services/studentService';

export const Dashboard = () => {
  const [students, setStudents] = useState<Student[]>([]);
  const [selectedStudent, setSelectedStudent] = useState<Student | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Busca os dados que o seu Hangfire sincronizou no banco
    studentService.getAll()
      .then(data => {
        setStudents(data);
        setLoading(false);
      })
      .catch(err => {
        console.error("Erro ao carregar estudantes", err);
        setLoading(false);
      });
  }, []);

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-6xl mx-auto">
        <header className="flex justify-between items-center mb-8">
          <h1 className="text-3xl font-bold text-gray-800">Estudantes Sincronizados</h1>
          <button 
            onClick={() => { localStorage.clear(); window.location.href='/login'; }}
            className="text-red-500 font-semibold hover:underline"
          >
            Sair
          </button>
        </header>

        {loading ? (
          <p>Carregando dados da API...</p>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {students.map(student => (
              <div key={student.id} className="bg-white p-6 rounded-xl shadow-sm border border-gray-200">
                <h2 className="text-xl font-bold text-blue-600 mb-2">{student.name}</h2>
                <p className="text-gray-600 mb-4">{student.email}</p>
                <button 
                  onClick={() => setSelectedStudent(student)}
                  className="w-full bg-gray-100 text-gray-700 py-2 rounded-lg font-medium hover:bg-gray-200 transition"
                >
                  Ver Agenda ({student.events?.length || 0})
                </button>
              </div>
            ))}
          </div>
        )}

        {/* Modal de Eventos */}
        {selectedStudent && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
            <div className="bg-white rounded-xl shadow-xl max-w-2xl w-full max-h-[80vh] overflow-y-auto">
              <div className="p-6 border-b border-gray-100 flex justify-between items-center">
                <h2 className="text-2xl font-bold text-gray-800">Agenda: {selectedStudent.name}</h2>
                <button 
                  onClick={() => setSelectedStudent(null)}
                  className="text-gray-400 hover:text-gray-600 text-2xl"
                >
                  &times;
                </button>
              </div>
              
              <div className="p-6">
                {selectedStudent.events && selectedStudent.events.length > 0 ? (
                  <ul className="space-y-4">
                    {selectedStudent.events.map((evt) => (
                      <li key={evt.id} className="border-l-4 border-blue-500 pl-4 py-2 bg-blue-50 rounded-r-md">
                        <h3 className="font-bold text-gray-800">{evt.subject}</h3>
                        <p className="text-sm text-gray-600">
                          {new Date(evt.startTime).toLocaleString()} - {new Date(evt.endTime).toLocaleTimeString()}
                        </p>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="text-center text-gray-500 py-8">Nenhum evento encontrado para este estudante.</p>
                )}
              </div>
              
              <div className="p-6 border-t border-gray-100 bg-gray-50 rounded-b-xl flex justify-end">
                <button 
                  onClick={() => setSelectedStudent(null)}
                  className="px-4 py-2 bg-gray-800 text-white rounded-lg hover:bg-gray-700"
                >
                  Fechar
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};