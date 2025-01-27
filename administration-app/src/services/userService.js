import axios from 'axios';
import { env } from '../config/env';
import jwtDecode from 'jwt-decode';

export function getAllUsers() {
	return axios(env.API_ENV.url + '/api/User/all', {
		method: 'GET',
		headers: {
			'Content-Type': 'application/json',
			Authorization: 'Bearer ' + localStorage.getItem('token'),
		},
	});
}

export function getUser() {
	return axios(env.API_ENV.url + '/api/User', {
		method: 'GET',
		headers: {
			'Content-Type': 'application/json',
			Authorization: 'Bearer ' + localStorage.getItem('token'),
		},
	});
}

export function getUserId() {
	const decoded = jwtDecode(localStorage.getItem('token'));
	return decoded.UserId;
}

export function getUserByName(username) {
	return axios(env.API_ENV.url + '/api/User/' + username, {
		method: 'GET',
		headers: {
			'Content-Type': 'application/json',
			Authorization: 'Bearer ' + localStorage.getItem('token'),
		},
	});
}

export function login(data) {
	return axios(env.API_ENV.url + '/api/User/login', {
		method: 'POST',
		data: JSON.stringify(data),
		headers: {
			'Content-Type': 'application/json',
		},
	});
}

export function twoFactorAuthentication(data) {
	return axios(env.API_ENV.url + '/api/User/login2FA', {
		method: 'POST',
		data: JSON.stringify(data),
		headers: {
			'Content-Type': 'application/json',
		},
	});
}

export function getTwoFactorQRCode() {
	return axios(env.API_ENV.url + '/api/User/2fa-qrcode', {
		method: 'GET',
		headers: {
			'Content-Type': 'application/json',
			Authorization: 'Bearer ' + localStorage.getItem('token'),
		},
	});
}

export function toggle2FA() {
	return axios(env.API_ENV.url + '/api/User/2fa-toggle', {
		method: 'PATCH',
		headers: {
			'Content-Type': 'application/json',
			Authorization: 'Bearer ' + localStorage.getItem('token'),
		},
	});
}
