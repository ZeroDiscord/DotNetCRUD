import React, { useState, useEffect } from 'react';
import { jwtDecode } from 'jwt-decode';

// --- Components ---
const Login = ({ setView, setToken }) => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');

    const handleLogin = async (e) => {
        e.preventDefault();
        setError('');

        try {
            const response = await fetch('http://localhost:5124/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    eventID: "1001",
                    addInfo: { username, password }
                })
            });

            const data = await response.json();

            if (data.rStatus === 200 && data.rData.token) {
                localStorage.setItem('token', data.rData.token);
                setToken(data.rData.token);
                setView('dashboard');
            } else {
                setError(data.rData.rMessage || 'Login failed.');
            }
        } catch (err) {
            setError('An error occurred. Please try again.');
        }
    };

    return (
        <div className="w-full max-w-md">
            <form onSubmit={handleLogin} className="bg-white shadow-lg rounded-xl px-8 pt-6 pb-8 mb-4">
                <h2 className="text-3xl font-bold text-center text-gray-800 mb-8">Welcome Back</h2>
                {error && <p className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-lg relative mb-4" role="alert">{error}</p>}
                <div className="mb-6">
                    <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="username">
                        Email or Username
                    </label>
                    <input
                        className="shadow-sm appearance-none border rounded-lg w-full py-3 px-4 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-green-500"
                        id="username"
                        type="text"
                        placeholder="example@example.com"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                    />
                </div>
                <div className="mb-6">
                    <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="password">
                        Password
                    </label>
                    <input
                        className="shadow-sm appearance-none border rounded-lg w-full py-3 px-4 text-gray-700 mb-3 leading-tight focus:outline-none focus:ring-2 focus:ring-green-500"
                        id="password"
                        type="password"
                        placeholder="******************"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>
                <div className="flex items-center justify-center">
                    <button
                        className="w-full bg-green-600 hover:bg-green-700 text-white font-bold py-3 px-4 rounded-lg focus:outline-none focus:shadow-outline transition duration-300"
                        type="submit">
                        Sign In
                    </button>
                </div>
                <p className="text-center text-gray-500 text-sm mt-6">
                    Don't have an account?{' '}
                    <button onClick={() => setView('register')} className="font-bold text-green-600 hover:text-green-800">
                        Register here
                    </button>
                </p>
            </form>
        </div>
    );
};

const Register = ({ setView }) => {
    const [name, setName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    const handleRegister = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');

        try {
            const response = await fetch('http://localhost:5124/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    eventID: "1000",
                    addInfo: { name, email, password }
                })
            });

            const data = await response.json();

            if (data.rStatus === 201) {
                setSuccess('Registration successful! Please log in.');
                setTimeout(() => setView('login'), 2000);
            } else {
                setError(data.rData.rMessage || 'Registration failed.');
            }
        } catch (err) {
            console.log(err);
            setError('An error occurred during registration. Please try again.');
        }
    };

    return (
        <div className="w-full max-w-md">
            <form onSubmit={handleRegister} className="bg-white shadow-lg rounded-xl px-8 pt-6 pb-8 mb-4">
                <h2 className="text-3xl font-bold text-center text-gray-800 mb-8">Create an Account</h2>
                {error && <p className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-lg relative mb-4" role="alert">{error}</p>}
                {success && <p className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded-lg relative mb-4" role="alert">{success}</p>}
                <div className="mb-4">
                    <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="name">
                        Full Name
                    </label>
                    <input className="shadow-sm appearance-none border rounded-lg w-full py-3 px-4 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-green-500" id="name" type="text" placeholder="Your Name" value={name} onChange={(e) => setName(e.target.value)} required />
                </div>
                <div className="mb-4">
                    <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="email">
                        Email
                    </label>
                    <input className="shadow-sm appearance-none border rounded-lg w-full py-3 px-4 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-green-500" id="email" type="email" placeholder="example@example.com" value={email} onChange={(e) => setEmail(e.target.value)} required />
                </div>
                <div className="mb-6">
                    <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="password-register">
                        Password
                    </label>
                    <input className="shadow-sm appearance-none border rounded-lg w-full py-3 px-4 text-gray-700 mb-3 leading-tight focus:outline-none focus:ring-2 focus:ring-green-500" id="password-register" type="password" placeholder="******************" value={password} onChange={(e) => setPassword(e.target.value)} required />
                </div>
                <div className="flex items-center justify-center">
                    <button className="w-full bg-green-600 hover:bg-green-700 text-white font-bold py-3 px-4 rounded-lg focus:outline-none focus:shadow-outline transition duration-300" type="submit">
                        Register
                    </button>
                </div>
                <p className="text-center text-gray-500 text-sm mt-6">
                    Already have an account?{' '}
                    <button onClick={() => setView('login')} className="font-bold text-green-600 hover:text-green-800">
                        Login here
                    </button>
                </p>
            </form>
        </div>
    );
};

const Dashboard = ({ token, setToken, setView }) => {
    const [user, setUser] = useState(null);
    const [isEditing, setIsEditing] = useState(false);
    const [newName, setNewName] = useState('');
    const [error, setError] = useState('');

    useEffect(() => {
        if (token) {
            try {
                const decoded = jwtDecode(token);
                setUser({
                    id: decoded.nameid,
                    name: decoded.name,
                    // You might want to add an API call here to get the email if it's not in the token
                });
                setNewName(decoded.name);
            } catch (e) {
                handleLogout();
            }
        }
    }, [token]);

    const handleLogout = () => {
        localStorage.removeItem('token');
        setToken(null);
        setView('login');
    };
    
    const handleUpdate = async (e) => {
        e.preventDefault();
        setError('');
        try {
             const response = await fetch('http://localhost:5124/updateUser', {
                method: 'POST',
                headers: { 
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    eventID: "2001",
                    addInfo: { id: user.id, name: newName }
                })
            });

            if(response.ok) {
                 const data = await response.json();
                 if(data.rStatus === 200) {
                    // Update successful, but we need a new token with the updated name
                    // For now, just update local state. In a real app, you'd re-login or get a new token.
                    setUser(prev => ({...prev, name: newName}));
                    setIsEditing(false);
                 } else {
                    setError(data.rData.rMessage || 'Update failed.');
                 }
            } else {
                setError('Failed to update user. Please try again.');
            }

        } catch(err) {
            setError('An error occurred during update.');
        }
    };
    
    const handleDelete = async () => {
        if(window.confirm('Are you sure you want to delete your account? This action cannot be undone.')) {
            try {
                 const response = await fetch('http://localhost:5124/deleteUser', {
                    method: 'POST',
                    headers: { 
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${token}`
                    },
                    body: JSON.stringify({
                        eventID: "3001",
                        addInfo: { id: user.id }
                    })
                });
                
                if(response.ok) {
                    alert('Account deleted successfully.');
                    handleLogout();
                } else {
                     alert('Failed to delete account.');
                }

            } catch(err) {
                alert('An error occurred while deleting the account.');
            }
        }
    };


    if (!user) {
        return <div className="text-white">Loading...</div>;
    }

    return (
        <div className="w-full max-w-2xl bg-white shadow-lg rounded-xl p-8">
            <div className="flex justify-between items-center mb-8">
                <h2 className="text-3xl font-bold text-gray-800">Profile Dashboard</h2>
                <button onClick={handleLogout} className="bg-gray-200 hover:bg-gray-300 text-gray-800 font-bold py-2 px-4 rounded-lg transition duration-300">
                    Logout
                </button>
            </div>
            
            {error && <p className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-lg relative mb-6" role="alert">{error}</p>}

            <div className="space-y-4">
                <div>
                    <span className="font-bold text-gray-600">User ID:</span>
                    <span className="ml-2 text-gray-800 bg-gray-100 px-3 py-1 rounded-md">{user.id}</span>
                </div>
            </div>

            <div className="mt-8 border-t pt-6">
                {isEditing ? (
                    <form onSubmit={handleUpdate}>
                         <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="newName">
                            New Name
                        </label>
                        <input
                            id="newName"
                            type="text"
                            value={newName}
                            onChange={e => setNewName(e.target.value)}
                            className="shadow-sm appearance-none border rounded-lg w-full py-3 px-4 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-green-500 mb-4"
                        />
                        <div className="flex gap-4">
                            <button type="submit" className="bg-green-500 hover:bg-green-600 text-white font-bold py-2 px-4 rounded-lg">Save Changes</button>
                            <button type="button" onClick={() => setIsEditing(false)} className="bg-gray-500 hover:bg-gray-600 text-white font-bold py-2 px-4 rounded-lg">Cancel</button>
                        </div>
                    </form>
                ) : (
                   <div className="flex flex-col sm:flex-row gap-4">
                        <button onClick={() => setIsEditing(true)} className="flex-1 bg-green-600 hover:bg-green-700 text-white font-bold py-2 px-4 rounded-lg transition duration-300">
                            Edit Profile
                        </button>
                         <button onClick={handleDelete} className="flex-1 bg-red-600 hover:bg-red-700 text-white font-bold py-2 px-4 rounded-lg transition duration-300">
                            Delete Account
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
};


export default function App() {
    const [view, setView] = useState('login'); // 'login', 'register', 'dashboard'
    const [token, setToken] = useState(localStorage.getItem('token'));

    useEffect(() => {
        if (token) {
            try {
                // Check if token is expired
                const decoded = jwtDecode(token);
                if (decoded.exp * 1000 < Date.now()) {
                    localStorage.removeItem('token');
                    setToken(null);
                    setView('login');
                } else {
                    setView('dashboard');
                }
            } catch (e) {
                localStorage.removeItem('token');
                setToken(null);
                setView('login');
            }
        } else {
            setView('login');
        }
    }, [token]);


    const renderView = () => {
        switch (view) {
            case 'register':
                return <Register setView={setView} />;
            case 'dashboard':
                return <Dashboard token={token} setToken={setToken} setView={setView} />;
            case 'login':
            default:
                return <Login setView={setView} setToken={setToken} />;
        }
    };

    return (
        <main className="bg-gray-100 min-h-screen flex flex-col items-center justify-center p-4">
             <div className="text-center mb-8">
                 <h1 className="text-5xl font-extrabold text-gray-800">User Dashboard</h1>
                 <p className="text-gray-500 mt-2">Powered by .NET & React</p>
             </div>
            {renderView()}
        </main>
    );
}
