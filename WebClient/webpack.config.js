const webpack = require('webpack');
const path = require('path');
const rxPaths = require('rxjs/_esm5/path-mapping');

// Webpack Plugins
const CleanWebpackPlugin = require('clean-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
const ManifestPlugin = require('webpack-manifest-plugin');

// Get npm lifecycle event to identify the environment
var ENV = process.env.npm_lifecycle_event;
if (ENV == 'build') {
  ENV = 'prod';
}

var isTestWatch = ENV === 'test-watch';
var isTest = ENV === 'test' || isTestWatch;
var isProd = ENV === 'prod';

var outputPath = path.resolve('../Website/src/wwwroot');

module.exports = () => {
  var config = {};

  if (isProd) {
    config.devtool = 'source-map';
  }
  else if (isTest) {
    config.devtool = 'inline-source-map';
  }
  else {
    config.devtool = 'cheap-module-eval-source-map';
  }

  if (!isTest) {
    config.entry = {
      'data': './src/data.ts',
      'polyfills': './src/polyfills.ts',
      'vendor': './src/vendor.ts',
      'app': './src/main.ts'
    };

    config.output = {
      path: outputPath,
      publicPath: '/',
      filename: isProd ? '[name].[chunkhash].js' : '[name].js',
      chunkFilename: isProd ? '[id].[chunkhash].chunk.js' : '[id].chunk.js'
    };
  }

  config.resolve = {
    extensions: ['.ts', '.js'],
    // Use the "alias" key to resolve to an ESM distribution. See https://github.com/ReactiveX/rxjs/blob/master/doc/lettable-operators.md#build-and-treeshaking
    alias: rxPaths()
  };

  config.module = {
    rules: [
      {
        test: /\.ts$/,
        enforce: 'pre',
        loader: 'tslint-loader',
        options: {
          configFile: path.resolve('./tslint.json'),
          tsConfigFile: path.resolve('./tsconfig.json'),
          emitErrors: true,
          failOnHint: true,
          typeCheck: true,
          fix: true,
        }
      },
      {
        test: /\.ts$/,
        loaders: [
          {
            loader: 'awesome-typescript-loader',
            options:
              {
                configFileName: path.resolve('./tsconfig.json')
              }
          },
          'angular2-template-loader'
        ]
      },
      {
        test: /\.html$/,
        loader: 'html-loader'
      },
      {
        test: /\.(png|jpe?g|gif|svg|woff|woff2|ttf|eot|ico)$/,
        loader: 'file-loader?name=assets/[name].[hash].[ext]'
      },
      {
        test: /\.css$/,
        loader: 'raw-loader'
      }
    ]
  };

  if (isTest && !isTestWatch) {
    config.module.rules.push(
      {
        test: /\.ts$/,
        use: {
          loader: 'istanbul-instrumenter-loader',
          options: { esModules: true }
        },
        enforce: 'post',
        exclude: /node_modules|\.spec\.ts$/,
      });
  }

  config.plugins = [
    new webpack.DefinePlugin({
      // Environment helpers
      'process.env': {
        ENV: JSON.stringify(ENV)
      }
    }),

    new webpack.optimize.ModuleConcatenationPlugin(),

    // Workaround for angular/angular#11580
    new webpack.ContextReplacementPlugin(
      /\@angular(\\|\/)core(\\|\/)esm5/,
      path.resolve('./src')
    ),
  ];

  if (!isTest) {
    config.plugins.push(
      new CleanWebpackPlugin([
        'app*.js',
        'app*.js.map',
        'data*.js',
        'data*.js.map',
        'polyfill*.js',
        'polyfill*.js.map',
        'runtime*.js',
        'runtime*.js.map',
        'vendor*.js',
        'vendor*.js.map',
        'index.html',
        'manifest.json',
      ], {
          root: outputPath,
          allowExternal: true
        }),

      // Creates hierarchy to keep code from one bundle out of others. See: https://angular.io/guide/webpack#commonschunkplugin
      new webpack.optimize.CommonsChunkPlugin({
        name: ['app', 'data', 'vendor', 'polyfills', 'runtime']
      }),

      new HtmlWebpackPlugin({
        template: 'src/index.html'
      }),

      new webpack.HashedModuleIdsPlugin(),

      new ManifestPlugin(),

      new BundleAnalyzerPlugin({
        analyzerMode: 'static',
        reportFilename: path.resolve('./logs/stats/report.html'),
        openAnalyzer: false,
        generateStatsFile: true,
        statsFilename: path.resolve('./logs/stats/stats.json'),
        statsOptions: {
          chunkModules: true // allows usage with webpack-visualizer
        },
        logLevel: 'silent'
      })
    );
  }

  if (isProd) {
    config.plugins.push(
      // Only emit files when there are no errors
      new webpack.NoEmitOnErrorsPlugin(),

      // Minify all javascript, switch loaders to minimizing mode
      new webpack.optimize.UglifyJsPlugin({
        sourceMap: true,
        // https://github.com/angular/angular/issues/10618
        mangle: {
          keep_fnames: true
        }
      }),

      new webpack.LoaderOptionsPlugin({
        htmlLoader: {
          minimize: false // workaround for ng2
        }
      })
    );
  }

  return config;
};
