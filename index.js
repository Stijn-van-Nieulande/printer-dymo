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
		assemblyFile: nodeDymoLib,
		typeName: 'NodeDymoLib.Dymo',
		methodName: 'Printers'
	});

	var print = module.exports.print = edge.func({
		assemblyFile: nodeDymoLib,
		typeName: 'NodeDymoLib.Dymo',
		methodName: 'Print'
	});
}

for (var f of dymoAssemblies) {
	var source = path.join(dymoLibPath, f), target = path.join(libDir, f);
	var readStream = fs.createReadStream(source);
	var writeStream = fs.createWriteStream(target);
	readStream.on('error', function(err) { throw err });
	writeStream.on('error', function(err) { throw err });
	writeStream.on('finish', initDymoLib);
	readStream.pipe(writeStream);
}


