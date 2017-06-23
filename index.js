var path = require('path'),
	fs = require('fs'),
	edge = require('electron-edge'),
	activePrinter,
	initReady = false;


//
// Lets Make sure the Libraries are here
var libDir = path.join(__dirname, 'lib'),
	nodeDymoLib = path.join(libDir, 'NodeDymoLib.dll'),
	dymoLibPath = path.join('C:', 'Program Files (x86)', 'DYMO', 'DYMO Label Software', 'Framework'),
	dymoAssemblies = [ 'DYMO.Label.Framework.dll', 'DYMO.DLS.Runtime.dll', 'DYMO.Common.dll' ],
	libsMoved = 0;

var initDymoLib = function() {
	if(++libsMoved < dymoAssemblies.length){ return false; }
	initReady = true;

	return true;
}

for( var f of dymoAssemblies ){
	if( fs.existsSync( path.join(libDir, f) ) ){ initDymoLib(); continue; } // Only fetch these files once
	console.log( 'Dymo Assembly ' + f );
	var source = path.join(dymoLibPath, f),
		target = path.join(libDir, f);
	var readStream = fs.createReadStream(source);
	var writeStream = fs.createWriteStream(target);
	readStream.on('error', function(err) { throw err });
	writeStream.on('error', function(err) { throw err });
	writeStream.on('finish', initDymoLib);
	readStream.pipe(writeStream);
}
// Lets Make sure the Libraries are here
//

var ready = module.exports.ready = function(){
	return initReady;
}


/** Get a list of all the printers
 * @param printerName printer name to extract the info
 * @return printer object info:
 */
 var getPrinters = module.exports.getPrinters = function( callback ){
	var availablePrinters = edge.func({
		assemblyFile: nodeDymoLib,
		typeName: 'NodeDymoLib.Dymo',
		methodName: 'Printers'
	});
	availablePrinters('', callback );
	return;
}

/**
 *	Same as getPrinters but syncronice
 */
 var getPrintersSync = module.exports.getPrintersSync = function(){
	if( initReady != true ){	}
	
	var availablePrinters = edge.func({
		assemblyFile: nodeDymoLib,
		typeName: 'NodeDymoLib.Dymo',
		methodName: 'Printers'
	});

	return availablePrinters( '', true );
}


var getPrinter = module.exports.getPrinterSync = function( thisPrinterName ){
	if( thisPrinterName == '' ){ return true; }
	
	var availablePrinters = edge.func({
		assemblyFile: nodeDymoLib,
		typeName: 'NodeDymoLib.Dymo',
		methodName: 'Printers'
	});

	var tempPrinters = availablePrinters( '', true );
	for( var i in tempPrinters ){
		console.log( tempPrinters[i] );
		if( tempPrinters[i].Name == thisPrinterName ){
			return tempPrinters[i];
		}
	}

	return false;
}
}


