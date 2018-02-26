const webpack = require('webpack');
const path = require('path');
const rxPaths = require('rxjs/_esm5/path-mapping');

// Webpack Plugins
const CleanWebpackPlugin = require('clean-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
const ManifestPlugin = require('webpack-manifest-plugin');
const AngularCompilerPlugin = require('@ngtools/webpack').AngularCompilerPlugin;
const ForkTsCheckerWebpackPlugin = require('fork-ts-checker-webpack-plugin');

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
  } else if (isTest) {
    config.devtool = 'inline-source-map';
  } else {
    config.devtool = 'cheap-module-eval-source-map';
  }

  config.mode = isProd ? "production" : "development";

  if (!isTest) {
    config.entry = {
      'data': './src/data.ts',
      'app': './src/main.ts'
    };

    config.optimization = {
      splitChunks: {
        chunks: "all",
      },
      runtimeChunk: "single",
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

  if (isProd) {
    config.module.rules.push({
      test: /(?:\.ngfactory\.js|\.ngstyle\.js|\.ts)$/,
      loader: '@ngtools/webpack'
    });
  } else {
    config.module.rules.push({
      test: /\.ts$/,
      loaders: [
        'cache-loader',
        {
          loader: 'thread-loader',
          options: {
            // there should be 1 cpu for the fork-ts-checker-webpack-plugin
            workers: require('os').cpus().length - 1
          }
        },
        {
          loader: 'ts-loader',
          options: {
            // IMPORTANT! use happyPackMode mode to speed-up compilation and reduce errors reported to webpack
            happyPackMode: true,
          }
        },
        'angular2-template-loader'
      ]
    });
  }

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
    // TS type checking and linting in parallel
    new ForkTsCheckerWebpackPlugin({
      checkSyntacticErrors: true,
      tslint: true,
    }),
  ];

  if (!isTest) {
    config.plugins.push(
      new CleanWebpackPlugin([
        '*.js',
        '*.js.map',
        'index.html',
        'manifest.json',
      ], {
          root: outputPath,
          allowExternal: true
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
      new AngularCompilerPlugin({
        tsConfigPath: './tsconfig.json',
        entryModule: './src/app.module#AppModule',
        sourceMap: true,
      }),

      new webpack.LoaderOptionsPlugin({
        htmlLoader: {
          minimize: false // workaround for ng2
        }
      })
    );
  } else {
    config.plugins.push(
      // Workaround for angular/angular#11580
      new webpack.ContextReplacementPlugin(
        /\@angular(\\|\/)core(\\|\/)esm5/,
        path.resolve('./src')
      )
    );
  }

  return config;
};
