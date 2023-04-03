import React, { useState } from 'react';
import { Toolbar, Tooltip, Typography, Button, Modal } from '@mui/material';
import PropTypes from 'prop-types';
import DeleteIcon from '@mui/icons-material/Delete';
import CreateIcon from '@mui/icons-material/Add';

import { alpha } from '@mui/material/styles';
import { Stack } from '@mui/system';
import VendorCreateModal from '../vendorCreateModal/VendorCreateModal';
import { makeStyles } from '@material-ui/core/styles';
import { createTheme, ThemeProvider } from '@mui/material/styles';

const useStyles = makeStyles(theme => ({
	button: {
		marginRight: '20px',
		width: '250px',
		'&.MuiButton-contained': {
			backgroundColor: '#ffaf36',
			color: 'black',
			'&:hover': {
				backgroundColor: '#ea8c00',
				boxShadow: 'none',
			},
			'&:disabled': {
				backgroundColor: '#ffffff',
				boxShadow: 'none',
				color: '#d3d3d3',
			},
		},
		'&.MuiButton-outlined': {
			color: '#ffaf36',
			border: '2px solid #ff9a00',

			'&:hover': {
				border: '2px solid #000000',
				color: '#000000',
			},
		},
	},
}));

const tableTheme = createTheme({
	palette: {
		primary: {
			main: '#E7EBF0',
		},
		secondary: {
			main: '#ff9a001f',
		},
		secondary2: {
			main: '#ffaf36',
		},
	},
});
export default function VendorsTableToolBar(props) {
	const classes = useStyles();
	const { numSelected } = props;
	const [open, setOpen] = useState(false);
	const handleOpen = () => setOpen(true);
	const handleClose = () => {
		props.fetchVendors();
		setOpen(false);
	};

	const createVendorsTooltip = (
		<Tooltip title='Create B2B Customer'>
			<Button className={classes.button} size='small' variant='contained' endIcon={<CreateIcon />} onClick={handleOpen}>
				Create B2B Customer
			</Button>
		</Tooltip>
	);
	const deleteVendorsTooltip = (
		<Tooltip title='Delete Selected B2B Customers'>
			<Button className={classes.button} size='small' variant='outlined' endIcon={<DeleteIcon />}>
				Delete B2B Customers
			</Button>
		</Tooltip>
	);

	return (
		<ThemeProvider theme={tableTheme}>
			<Toolbar
				sx={{
					pl: { sm: 2 },
					pr: { xs: 1, sm: 1 },
					...(numSelected > 0 && {
						bgcolor: theme => alpha(theme.palette.secondary.main, theme.palette.action.activatedOpacity),
					}),
				}}>
				{numSelected > 0 ? (
					<Typography sx={{ flex: '1 1 100%' }} color='inherit' variant='subtitle1' component='div'>
						{numSelected} selected
					</Typography>
				) : (
					<Typography sx={{ flex: '1 1 100%' }} variant='h6' id='tableTitle' component='div'>
						B2B Customers
					</Typography>
				)}

				{numSelected > 0 ? (
					<Stack direction='row' spacing={1}>
						{deleteVendorsTooltip}
						{createVendorsTooltip}
					</Stack>
				) : (
					<Stack direction='row' spacing={0}>
						{createVendorsTooltip}
					</Stack>
				)}

				<Modal
					open={open}
					onClose={handleClose}
					aria-labelledby='modal-modal-title'
					aria-describedby='modal-modal-description'>
					<VendorCreateModal handleClose={handleClose} />
				</Modal>
			</Toolbar>
		</ThemeProvider>
	);
}

VendorsTableToolBar.propTypes = {
	numSelected: PropTypes.number.isRequired,
};