import React from 'react';
import LoginForm from './components/Login/Login';
import ProfilePage from './components/User/UserProfile';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import VendorsTable from './components/vendor/vendorsPanel/VendorsTable';
import Location from './components/vendor/Location/Location';
import ProtectedRoute from './components/ProtectedRoute';
import { NavBar } from './components/NavBar/NavBar';
import { TransactionsList } from './components/TransactionsView/transactions/TransactionsList';
import { Payment } from './components/Payment/Payment';
import { useState } from 'react';

import './App.css';

function App() {
	const [token, setToken] = useState(null);
	return (
		<div className='App'>
			<Router>
				<NavBar token={token} setToken={setToken} />
				<Routes>
					<Route path='/' element={<h1 style={{ textAlign: 'center' }}>SI projekat</h1>} />
					<Route
						path='/transactions'
						element={
							<ProtectedRoute>
								<TransactionsList />
							</ProtectedRoute>
						}
					/>
					<Route
						path='/payment/:currency/:type/:recipientName/:transactionAmount/:recipientAccountNumber'
						element={
							<ProtectedRoute>
								<Payment />
							</ProtectedRoute>
						}
					/>
					<Route
						path='/payment'
						element={
							<ProtectedRoute>
								<Payment />
							</ProtectedRoute>
						}
					/>

					<Route
						path='/user'
						element={
							<ProtectedRoute>
								<ProfilePage setToken={setToken} />
							</ProtectedRoute>
						}
					/>
					<Route
						path='/vendor-management'
						element={
							<ProtectedRoute>
								<VendorsTable />
							</ProtectedRoute>
						}
					/>
					<Route
						path='/B2BLocation/:id'
						element={
							<ProtectedRoute>
								<Location />
							</ProtectedRoute>
						}
					/>
					<Route path='/login' element={<LoginForm setToken={setToken} />} />
				</Routes>
			</Router>
		</div>
	);
}
export default App;
