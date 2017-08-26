var webpack = require('webpack');
var path = require('path');

// Webpack Plugins
var HtmlWebpackPlugin = require('html-webpack-plugin');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;

// Get npm lifecycle event to identify the environment
var ENV = process.env.npm_lifecycle_event;
if (ENV == 'build')
{
  ENV = 'prod';
}

var isTestWatch = ENV === 'test-watch';
var isTest = ENV === 'test' || isTestWatch;
var isProd = ENV === 'prod';

module.exports = () =>
{
  var config = {};

  if (isProd)
  {
    config.devtool = 'source-map';
  }
  else if (isTest)
  {
    config.devtool = 'inline-source-map';
  }
  else
  {
    config.devtool = 'cheap-module-eval-source-map';
  }

  if (!isTest)
  {
    config.entry = {
      'data': './src/data.ts',
      'polyfills': './src/polyfills.ts',
      'vendor': './src/vendor.ts',
      'app': './src/main.ts'
    };

    config.output = {
      path: path.resolve('./dist'),
      publicPath: '/',
      filename: isProd ? '[name].[hash].js' : '[name].js',
      chunkFilename: isProd ? '[id].[hash].chunk.js' : '[id].chunk.js'
    };
  }

  config.resolve = {
    extensions: ['.ts', '.js']
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

  if (isTest && !isTestWatch)
  {
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

    // Workaround for angular/angular#11580
    new webpack.ContextReplacementPlugin(
      // The (\\|\/) piece accounts for path separators in *nix and Windows
      /angular(\\|\/)core(\\|\/)@angular/,

      // location of app
      path.resolve('./src')
    ),
  ];

  if (!isTest)
  {
    config.plugins.push(
      // Creates hierarchy to keep code from one bundle out of others. See: https://angular.io/guide/webpack#commonschunkplugin
      new webpack.optimize.CommonsChunkPlugin({
        name: ['app', 'data', 'vendor', 'polyfills']
      }),

      new HtmlWebpackPlugin({
        template: 'src/index.html'
      }),

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

  if (isProd)
  {
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
